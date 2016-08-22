from Crypto.PublicKey import RSA

private = RSA.generate(1024, e=3)
public = private.publickey()


f = open ('public.pem', 'w')
f.write(public.exportKey()) #write ciphertext to file
f.close()

f = open ('private.pem', 'w')
f.write(private.exportKey()) #write ciphertext to file
f.close()