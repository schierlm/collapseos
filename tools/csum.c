#include <stdio.h>
/* Compute checksums from stdin. */

int main(int argc, char **argv) {
    unsigned int sz, sum, crc16;
    int c;

    sz = sum = crc16 = 0;
    c = getchar();
    while (c != EOF) {
        sz++;
        sum += c;
        crc16 = crc16 ^ c << 8;
        for (int i=0; i<8; i++) {
            if (crc16 & 0x8000) {
                crc16 = crc16 << 1 ^ 0x1021;
            } else {
                crc16 = crc16 << 1;
            }
        }
        c = getchar();
    }
    printf("Size: %04x\n", sz);
    printf("Simple sum: %04x (%d, %d)\n", sum & 0xffff, sum & 0xffff, sum);
    printf("CRC16: %04x\n", crc16 & 0xffff);

}
