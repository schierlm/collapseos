void sendcmd(int fd, char *cmd);
void sendcmdp(int fd, char *cmd);
void mread(int fd, char *s, int count);
int mexpect(int fd, char ec);
void readprompt(int fd);
void set_blocking(int fd, int should_block);
int ttyopen(char *devname);
