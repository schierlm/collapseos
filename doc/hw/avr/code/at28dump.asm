; Use this as a debug companion to at28wr. This simply dumps, TTY-escaped, the
; contents of the AT28.
;
; TODO: not always, but sometimes, the output starts with a spurious 0xFF. But
; otherwise, the rest of the contents is good, albeit offset by 1 (that is, the
; byte after the spurious 0xFF is the contents at addr 0). Weird, to fix.

.include "m328Pdef.inc"

; *** Pins ***
.equ SRCP = PORTB2
.equ SRDS = PORTB1
.equ FLWE = PORTB3
.equ FLOE = PORTB4
.equ FLCE = PORTB5 ; WARNING: same as LED

; *** Consts ***
.equ BAUD_PRESCALE = 103	; 9600 bauds at 16mhz

rjmp    main

; *** Code ***
; Sends char in r20 to UART
; Perform TTY-escape transparently.
uartwr:
	lds	r16, UCSR0A
	sbrs    r16, UDRE0	; wait until send buffer is empty
	rjmp	uartwr
	; should we escape?
	cpi	r20, 0x21
	brsh	uartwr_0	; r20 >= 0x21, skip
	; escape
	ori	r20, 0x80
	ldi	r16, 0x20
	sts	UDR0, r16
	rjmp	uartwr
uartwr_0:
	sts	UDR0, r20
	ret

; send r23 to addr shift register.
; We send highest bits first so that Q7 is the MSB and Q0 is the LSB
sendaddr:
	ldi	r16, 8		; we will loop 8 times
	cbi	PORTB, SRDS
	sbrc	r23, 7		; if latest bit isn't cleared, set SER_DP high
	sbi	PORTB, SRDS
	; toggle SRCP, not waiting between pulses. The CD74AC164 at 5V has a
        ; 5.9ns CP min pulse width. We can't match that at 16mhz. No need to
	; wait.
	cbi	PORTB, SRCP
	sbi	PORTB, SRCP
	lsl	r23		; shift our data left
	dec	r16
	brne	sendaddr+1	; not zero yet? loop! (+1 to avoid reset)
	ret

; push r20 to the rom and increase the memory counter
nextaddr:
	; first, set up addr
	mov	r23, r21
	rcall	sendaddr
	mov	r23, r22
	rcall	sendaddr
	inc	r22
	brne	nextaddr_0	; no overflow? skip
	inc	r21
nextaddr_0:
	ret

; read EEPROM's I/O7:0 through PD7:2 and PB1:0 into r20
readdata:
	cbi	PORTB, FLCE
	cbi	PORTB, FLOE
	nop 			; 70ns max delay on at28
	nop
	nop
	nop
	; read bits 7:2
	in	r20, PIND
	andi	r20, 0xfc
	; read bits 1:0
	in	r16, PINB
	andi	r16, 0x03
	or	r20, r16
	sbi	PORTB, FLOE
	sbi	PORTB, FLCE
	ret

; Set PD7:2 and PB1:0 to output
ioout:
	ldi	r16, 0xfc	; PD7:2
	out	DDRD, r16
	ldi	r16, 0x3f	; PB5:0 (CP, WE, OE and CE too)
	out	DDRB, r16
	ret

; Set PD7:2 and PB1:0 to input
ioin:
	ldi	r16, 0x03	; PD7:2
	out	DDRD, r16
	ldi	r16, 0x3c	; PB1:0
	out	DDRB, r16
	ret

main:
	ldi	r16, low(RAMEND)
	out	SPL, r16
	ldi	r16, high(RAMEND)
	out	SPH, r16

	; We begin with WE and OE disabled (high), but CE stays enabled (low)
	; the whole time.
	sbi	PORTB, FLWE
	sbi	PORTB, FLOE
	sbi	PORTB, FLCE

	; Clear counters and flags
	clr	r21
	clr	r22

	; Setup UART
	ldi	R16, low(BAUD_PRESCALE)
	sts	UBRR0L, r16
	ldi	r16, high(BAUD_PRESCALE)
	sts	UBRR0H, r16

	ldi	r16, (1<<TXEN0)
	sts	UCSR0B, r16

loop:
	rcall	ioout
	rcall	nextaddr
	rcall	ioin
	rcall	readdata
	rcall	uartwr
	rjmp	loop


