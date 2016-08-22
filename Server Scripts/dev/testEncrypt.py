#testDecrypt

from Crypto.PublicKey import RSA
from base64 import b64decode


fOpen = open('key.txt', 'r')
longkey = bytes(fOpen.read())

rsakey = RSA.importKey(b64decode(longkey))

testEncrypt = "ZSS+SrNbUXkICFshFUKR6bjaMPDgv5FPAXq2FWVX6txqwvyZCqfUCjyhqflLsscp3Fa/OkS8uhY9lEDK3ABdtyAt1Sm74lBs+n29PLthYbUxayW+TqkMJ0f+cNFTFwUQ7K+pNx9I4rYPAeMXDnXzO3E2QeAhatsoD1su68kp/CI="

raw_cipher_data = b64decode(testEncrypt)

decrypted = rsakey.decrypt(raw_cipher_data)

print decrypted