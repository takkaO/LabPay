#include <crypt.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>

int main(){
	char *key = "hoge";
	char *salt = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF";
	char *p;

	crypt_set_format("sha512");
	p = crypt(key, salt);
	printf("%s\n", p);
}