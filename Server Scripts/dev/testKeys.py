from Crypto.PublicKey import RSA
from base64 import b64decode

fOpen = open('private.pem', 'r')
privateKey = fOpen.read()

#Import the private key
rsakey = RSA.importKey(privateKey)

testEncrypt = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAktI4w/iOPCY/g1HV6Nf1mcexE4b0PM7kintiRJO7RG331ViCEn4asE/pPVKoztJH98/GoxjtAQeeIn2ptWdw=="

raw_cipher_data = b64decode(testEncrypt)

decrypted = rsakey.decrypt(raw_cipher_data)

print decrypted