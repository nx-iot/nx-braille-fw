for x in xrange(0,255):
    ic=x
    print(hex(ic)+" "+unichr(ic))

for x in xrange(0,255):
    ic=x+3584
    print(hex(ic)+" "+unichr(ic))
    #print(hex(ic))
	#print("".join("%02x" % (x+3584)).join(":"))
    
    #.join(unichr(x+3584)))