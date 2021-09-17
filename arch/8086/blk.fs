( ----- 000 )
8086 MASTER INDEX

301 8086 boot code            320 8086 drivers
( ----- 001 )
\ 8086 boot code. PS=SP, RS=BP, IP=DX
: 8086C 302 305 LOADR ;
: 8086H 306 310 LOADR ;
1 VALUE JROPLEN -1 VALUE JROFF
( ----- 002 )
HERE TO ORG
FJR JRi, TO L1 ( main ) 0 C, 0 C, ( 03, boot driveno )
8 ALLOT0
\ End of Stable ABI
L1 FMARK ( main ) DX POPx, ( boot drive no ) $03 DL MOVmr,
  SP PS_ADDR MOVxI, BP RS_ADDR MOVxI,
  DI $04 ( BOOT ) MOVxm, DI JMPr,
LSET lbldoes BP INCx, BP INCx, [BP] 0 DX []+x MOV[], ( pushRS )
  DX AX MOVxx, \ continue to lblcell
LSET lblcell AX POPx, \ continue to lblpush
LSET lblpush PUSHp, \ continue to lblnext
LSET lblnext DI DX MOVxx, ( <-- IP ) DX INCx, DX INCx,
  DI [DI] x[] MOV[], DI JMPr,
LSET lblxt BP INCx, BP INCx, [BP] 0 DX []+x MOV[], ( pushRS )
  DX POPx, lblnext BR JRi,
( ----- 003 )
CODE * AX POPx,
  DX PUSHx, ( protect from MUL ) BX MULx, DX POPx,
  BX AX MOVxx, ;CODE
CODE /MOD AX POPx, DX PUSHx, ( protect )
  DX DX XORxx, BX DIVx,
  BX DX MOVxx, DX POPx, ( unprotect )
  BX PUSHx, ( modulo ) BX AX MOVxx, ( division ) ;CODE
CODE []= ( a1 a2 u -- f ) CX BX MOVxx, SI POPx, DI POPx,
  CLD, REPZ, CMPSB, BX 0 MOVxI, IFZ, BX INCx, THEN, ;CODE
CODE QUIT LSET L1 ( used in ABORT )
  BP RS_ADDR MOVxI, DI $0a ( main ) MOVxm, DI JMPr,
CODE ABORT SP PS_ADDR MOVxI, L1 BR JRi,
CODE BYE HLT, BEGIN, BR JRi,
CODE RCNT RS_ADDR i>w, PUSHp, AX BP MOVxx, -wp, w>p, ;CODE
CODE SCNT AX SP MOVxx, PUSHp, PS_ADDR i>w, -wp, w>p, ;CODE
( ----- 004 )
CODE FIND ( sa sl -- w? f ) CX BX MOVxx, SI POPx,
  DI SYSVARS $2 ( CURRENT ) + MOVxm,
  BEGIN, ( loop )
    AL [DI] -1 r[]+ MOV[], $7f ANDALi, ( strlen )
    CL AL CMPrr, IFZ, ( same len )
      SI PUSHx, DI PUSHx, CX PUSHx, ( --> )
        3 ADDALi, ( header ) AH AH XORrr, DI AX SUBxx,
        CLD, REPZ, CMPSB,
      CX POPx, DI POPx, SI POPx, ( <-- )
      IFZ, DI PUSHx, BX 1 MOVxI, ;CODE THEN,
    THEN,
    DI 3 SUBxi, DI [DI] x[] MOV[], ( prev ) DI DI ORxx,
  Z? ^? BR ?JRi, ( loop ) BX BX XORxx, ;CODE
( ----- 005 )
( See comment in B294 TODO: test on real hardware. in qemu,
  the resulting delay is more than 10x too long. )
CODE TICKS ( n=100us ) BX PUSHx,
    SI DX MOVxx, ( protect IP )
    AX POPx, BX 100 MOVxI, BX MULx,
    CX DX MOVxx, ( high ) DX AX MOVxx, ( low )
    AX $8600 MOVxI, ( 86h, WAIT ) $15 INT,
    DX SI MOVxx, ( restore IP ) BX POPx, ;CODE
( ----- 006 )
: w>p, $89c3 M, ; \ mov bx,ax
: p>w, $89d8 M, ; \ mov ax,bx
: DROPp, $5b C, ( pop bx ) ; : POPp, p>w, DROPp, ;
: DUPp, $53 C, ( push bx ) ; : PUSHp, DUPp, w>p, ;
: POPf, $58 C, ( pop ax ) ; : PUSHf, $50 C, ( push ax ) ;
: POPr, $8b46 M, $00 C, ( mov ax,[bp+0] )
  $4d4d M, ( dec bp;dec bp ) ;
: PUSHr, $4545 M, ( inc bp;inc bp )
  $8946 M, $00 C, ( mov [bp+0],ax ) ;
: SWAPwp, $93 C, ( xchg ax,bx ) ;
: SWAPwf, $89c1 M, ( mov cx,ax ) POPf, $51 C, ( push cx ) ;
( ----- 007 )
SYSVARS $16 + *VALUE ?JROP
: JMPw, $ffe0 M, ; \ jmp ax
: JMPi, $e9 C, ( jmp near ) PC - 2 - L, ;
: CALLi, $e8 C, ( jmp near ) PC - 2 - L, ;
: JRi, $eb C, ( jmp short ) C, ;
: ?JRi, ?JROP C, C, ;
: INCw, $40 C, ( inc ax ) ; : DECw, $48 C, ( dec ax ) ;
: INCp, $43 C, ( inc bx ) ; : DECp, $4b C, ( dec bx ) ;
: i>w, $b8 C, L, ; \ mov ax,nn
: (i)>w, $a1 C, L, ; \ mov ax,(nn)
: C@w, $89c7 M, ( mov di,ax ) $30e4 M, ( xor ah,ah )
  $8a05 M, ( mov al,[di] ) ;
: @w, $89c7 M, ( mov di,ax ) $8b05 M, ( mov ax,[di] ) ;
: C!wp, $89c7 M, ( mov di,ax ) $881d M, ( mov [di],bl ) ;
: !wp, $89c7 M, ( mov di,ax ) $891d M, ( mov [di],bx ) ;
( ----- 008 )
: w>Z, $09c0 M, ( or ax,ax ) ;
: p>Z, $09db M, ( or bx,bx ) ;
: Z? $74 [*TO] ?JROP ; : C? $72 [*TO] ?JROP ;
: ^? ?JROP 1 XOR [*TO] ?JROP ;
: C>w, $b8 C, 0 L, ( mov ax,0 ) $1400 M, ( adc al,0 ) ;
: Z>w, $b8 C, 0 L, $7501 M, ( jrnz+1 ) INCw, ;
: IP>w, $89d0 M, ; \ mov ax,dx
: w>IP, $89c2 M, ; \ mov dx,ax
: IP+, $42 C, ; \ inc dx
: IP+off, $50 C, ( push ax ) $89d7 M, ( mov di,dx )
  $30e4 M, ( xor ah,ah ) $8a05 M, ( mov al,[di] )
  $98 C, ( cbw ) $01c2 M, ( add dx,ax ) $58 C, ( pop ax ) ;
( ----- 009 )
: +wp, $01d8 M, ( add ax,bx ) ;
: -wp, $29d8 M, ( sub ax,bx ) ;
: >>w, $d1e8 M, ; \ shr ax
: <<w, $d1e0 M, ; \ shl ax
: >>8w, $88e0 M, $30e4 M, ; \ mov al,ah;xor ah,ah
: <<8w, $88c4 M, $30c0 M, ; \ mov ah,al;xor al,al
( ----- 010 )
: CMPwp, $39d8 M, ( cmp ax,bx ) ;
: ANDwp, $21d8 M, ( and ax,bx ) ;
: ORwp, $09d8 M, ( or ax,bx ) ;
: XORwp, $31d8 M, ( xor ax,bx ) ;
: XORwi, $35 C, L, ( xor ax,nn ) ;
( ----- 020 )
( PC/AT drivers. Load range: 320-326 )
CODE (key?)
  AH AH XORrr, $16 INT, AH AH XORrr, PUSHp, DUPp, ;CODE
( ----- 021 )
CODE 13H08H ( driveno -- cx dx )
  DX PUSHx, ( protect ) DX BX MOVxx, AX $800 MOVxI,
  ES PUSHs, DI DI XORxx, ES DI MOVsx,
  $13 INT, BX DX MOVxx, ES POPs, DX POPx, ( unprotect )
  CX PUSHx, ;CODE
CODE 13H ( ax bx cx dx -- ax bx cx dx )
  SI BX MOVxx, ( DX ) CX POPx, BX POPx, AX POPx,
  DX PUSHx, ( protect ) DX SI MOVxx, DI DI XORxx,
  $13 INT, SI DX MOVxx, DX POPx, ( unprotect )
  AX PUSHx, BX PUSHx, CX PUSHx, BX SI MOVxx, ;CODE
( ----- 022 )
DRV_ADDR VALUE FDSPT
DRV_ADDR 1+ VALUE FDHEADS
: _ ( AX BX sec )
  ( AH=read sectors, AL=1 sector, BX=dest,
    CH=trackno CL=secno DH=head DL=drive )
  FDSPT C@ /MOD ( AX BX sec trk )
  FDHEADS C@ /MOD ( AX BX sec head trk )
  <<8 ROT OR 1+ ( AX BX head CX )
  SWAP <<8 $03 C@ ( boot drive ) OR ( AX BX CX DX )
  13H 2DROP 2DROP ;
( ----- 023 )
\ Sectors are 512b, so blk numbers are all x2. We add 16 to
\ this because blkfs starts at sector 16.
: FD@ ( blkno blk( -- )
  SWAP << ( 2* ) 16 + 2DUP ( a b a b )
  $0201 ROT> ( a b c a b ) _ ( a b )
  1+ SWAP $200 + SWAP $0201 ROT> ( c a b ) _ ;
: FD! ( blkno blk( -- )
  SWAP << ( 2* ) 16 + 2DUP ( a b a b )
  $0301 ROT> ( a b c a b ) _ ( a b )
  1+ SWAP $200 + SWAP $0301 ROT> ( c a b ) _ ;
: FD$
\ get number of sectors per track with command 08H.
  $03 ( boot drive ) C@ 13H08H
  >>8 1+ FDHEADS C!
  $3f AND FDSPT C! ;
( ----- 024 )
2 VALUES COLS 80 LINES 25
CODE CURSOR! ( new old ) AX POPx, ( new ) DX PUSHx, ( protect )
  BX 80 MOVxI, DX DX XORxx, BX DIVx, ( col in DL, row in AL )
  DH AL MOVrr, AH 2 MOVri,
  $10 INT, DX POPx, ( unprotect ) BX POPx, ;CODE
CODE _spit ( c )
  POPp, AH $0e MOVri, ( print char ) $10 INT, ;CODE
: CELL! ( c pos -- ) 0 CURSOR! _spit ;
