# -*- coding: UTF-8 -*-
#brailleDisplayDrivers/thaiBraille.py
#A part of NonVisual Desktop Access (NVDA)
#This file is covered by the GNU General Public License.
#See the file COPYING for more details.
#Copyright (C) 2014-2015 ONCE-CIDAT <cidat.id@once.es>

import inputCore
import braille
import hwPortUtils
from collections import OrderedDict
from logHandler import log
import serial
import struct
import wx


READ_INTERVAL = 50
TIMEOUT = 0.1



class thaiTypes:
	TTHAI_NO_DISPLAY = 0
	TTHAI_20 = 20
	TTHAI_40 = 40
	TTHAI_80 = 80

def thai_in_init(dev):
	#msg = dev.read(13)
	#if (len(msg) < 13):
	#	return thaiTypes.TTHAI_40 # Needed to restart NVDA with Ecoplus
	#msg = struct.unpack('BBBBBBBBBBBBB', msg)
	# Command message from ThaiBraille is something like that:
	# 0x10 0x02 TT AA BB CC DD 0x10 0x03
	# where TT can be 0xF1 (identification message) or 0x88 (command pressed in the line)
	# If TT = 0xF1, then the next byte (AA) give us the type of ThaiBraille line (ECO 80, 40 or 20)
	#if (msg[0] == 0x10) and (msg[1] == 0x02) and (msg[7] == 0x10) and (msg[8] == 0x03):
	#	if msg[2] == 0xf1: # Initial message
	#		if (msg[3] == 0x80):
	#			return thaiTypes.TTHAI_80
	#		if (msg[3] == 0x40):
	#			return thaiTypes.TTHAI_40
	#		if (msg[3] == 0x20):
	#			return thaiTypes.TTHAI_20
	return thaiTypes.TTHAI_40 # Needed for changing Braille Settings with Ecoplus

def thai_in(dev):
	msg = dev.read(13)
	try:
		msg = struct.unpack('BBBBBBBBBBBBB', msg)
	except:
		return 0
	# Command message from EcoBraille is something like that:
	# 0x10 0x02 TT AA BB CC DD 0x10 0x03
	# where TT can be 0xF1 (identification message) or 0x88 (command pressed in the line)
	# If TT = 0x88 then AA, BB, CC and DD give us the command pressed in the braille line
	
	
	if (len(msg) < 13):
		return 0
		
	#log.info(msg)
	hex_string = "".join(" %02x " % b for b in msg)
	log.info(hex_string)

	if (msg[0] == 0x10) and (msg[1] == 0x02) and (msg[11] == 0x10) and (msg[12] == 0x03):
		return msg
	return 0



def thai_out(cells):
	# Messages sends to thaiBraille display are something like that:
	# 0x10 0x02 0xBC message 0x10 0x03
	ret = bytearray()
	ret.append(0x10)
	ret.append(0x02)
	ret.append(0xBC)
	ret.append(0x00)
	ret.append(0x00)
	ret.append(0x00)
	ret.append(0x00)
	ret.append(0x00)
	#ret.append(struct.pack('BBB','\x10', '\x02', '\xBC'))
	# Leave status cells blank
	#ret.append(struct.pack('BBBBB', '\x00', '\x00', '\x00', '\x00', '\x00'))
	for d in cells:
		ret.append(d)
	#ret.append(struct.pack('BB', '\x10', '\x03'))
	ret.append(0x00)
	ret.append(0x00)
	ret.append(0x00)
	ret.append(0x00)
	ret.append(0x00)
	ret.append(0x00)
	ret.append(0x00)
	ret.append(0x10)
	ret.append(0x03)
	#ret.append(struct.pack('BBBBB', 0x53, 0x11,0x07,0x07,0x15))
	hex_string = "".join(" %02x " % b for b in ret)
	log.info(hex_string)
	return ret
	#"".join(ret)

class BrailleDisplayDriver(braille.BrailleDisplayDriver):
	""" Thai Braille display driver.
	"""
	name = "thaiBraille"
	# Translators: The name of a braille display.
	description = _("ThaiBraille displays")
	
	@classmethod
	def check(cls):
		return True

	@classmethod
	def getPossiblePorts(cls):
		ports = OrderedDict()
		for p in hwPortUtils.listComPorts():
			# Translators: Name of a serial communications port.
			ports[p["port"]] = _("Serial: {portName}").format(portName=p["friendlyName"])
		return ports

	def __init__(self, port):
		super(BrailleDisplayDriver, self).__init__()
		
		
		self._port = (port)
		# Try to open port
		self._dev = serial.Serial(self._port, baudrate = 9600,  bytesize = serial.EIGHTBITS, parity = serial.PARITY_NONE, stopbits = serial.STOPBITS_ONE)
		# Use a longer timeout when waiting for initialisation.
		self._dev.timeout = self._dev.writeTimeout = 2.7
		self._thaiType  = thai_in_init(self._dev)
		# Use a shorter timeout hereafter.
		self._dev.timeout = self._dev.writeTimeout = TIMEOUT
		# Always send the protocol answer.
		
		#self._dev.write("\x61\x10\x02\xf1\x57\x57\x57\x10\x03")
		#self._dev.write("\x10\x02\xbc\x00\x00\x00\x00\x00\x10\x03")
		
		# Start keyCheckTimer.
		self._readTimer = wx.PyTimer(self._handleResponses)
		self._readTimer.Start(READ_INTERVAL)

	def terminate(self):
		super(BrailleDisplayDriver, self).terminate()
		try:
			self._dev.write("\x61\x10\x02\xf1\x57\x57\x57\x10\x03")
			self._readTimer.Stop()
			self._readTimer = None
		finally:
			self._dev.close()
			self._dev = None

	def _get_numCells(self):
		return self._thaiType

	def display(self, cells):
		try:
		#	cells = "".join(chr(cell) for cell in cells)
			self._dev.write(thai_out(cells))
			#self._dev.write(cells)
		except:
			pass

	def _handleResponses(self):
		if self._dev.inWaiting():
			command = thai_in(self._dev)
			if command:
				try:
					self._handleResponse(command)
				except KeyError:
					pass
    
	
	def _handleResponse(self, command):
		if(command[2] > 0x00 and command[9] > 0x00 ):
			#Space + Braille Key Command
			cmd_string = "".join("S%02x" % command[2])
			log.info(cmd_string)
			inputCore.manager.executeGesture(InputGestureKeys(cmd_string))
		elif (command[4] > 0x00 ):
			# Command Key
			cmd_string = "".join("C%02x" % command[4])
			log.info(cmd_string)
			inputCore.manager.executeGesture(InputGestureKeys(cmd_string))
		elif (command[10] > 0x00 ):
			# Allow Key
			cmd_string = "".join("A%02x" % command[11])
			log.info(cmd_string)
			inputCore.manager.executeGesture(InputGestureKeys(cmd_string))
		elif (command[5] > 0x00 or command[6] > 0x00 or command[7] > 0x00 or command[8] > 0x00 or command[9] > 0x00):
			# Allow Key
			cmd_string = "".join("P%02x" % command[5]).join("%02x" % command[6]).join("%02x" % command[7]).join("%02x" % command[8]).join("%02x" % command[9])
			log.info(cmd_string)
			inputCore.manager.executeGesture(InputGestureKeys(cmd_string))
		else:
			#Routing
			inputCore.manager.executeGesture(InputGestureRouting(command[2]))
		return 0
			
		#if command in (THAI_KEY_STATUS1, THAI_KEY_STATUS2, THAI_KEY_STATUS3, THAI_KEY_STATUS4):
			# Nothing to do with the status cells
		#	return 0
		#if (command < THAI_END_ROUTING) and (command >= THAI_START_ROUTING):
			# Routing
		#	try:
		#		inputCore.manager.executeGesture(InputGestureRouting(((command - THAI_START_ROUTING) >> 64) + 1))
		#	except:
		#		log.info("THAIBraille: No function associated with this routing key {key}".format(key=command))
		#elif command > 0:
			# Button
		#	try:
		#		inputCore.manager.executeGesture(InputGestureKeys(command))
		#	except inputCore.NoInputGestureAction:
		#		log.info("THAIBraille: No function associated with this Braille key {key}".format(key=command))
		#return 0

	gestureMap = inputCore.GlobalGestureMap({
		"globalCommands.GlobalCommands": {
			"braille_routeTo": ("br(thaiBraille):routing"),
			"braille_cycleCursorShape":("br(thaiBraille):F1"),
			"braille_nextLine": ("br(thaiBraille):A04"), 
			"braille_previousLine": ("br(thaiBraille):A02"),
			"braille_scrollBack": ("br(thaiBraille):A01"),
			"braille_scrollForward": ("br(thaiBraille):A03"),
			"braille_toFocus": ("br(thaiBraille):C20"),
			"braille_toggleShowCursor": ("br(thaiBraille):F7"),
			"braille_toggleTether": ("br(thaiBraille):F8"),

			"reviewMode_previous": ("br(thaiBraille):F10"),
			"navigatorObject_parent": ("br(thaiBraille):F9"),
			"navigatorObject_previous": ("br(thaiBraille):S0F"),
			"navigatorObject_current": ("br(thaiBraille):F12"),
			"navigatorObject_next": ("br(thaiBraille):S1D"),
			"navigatorObject_toFocus": ("br(thaiBraille):F14"),
			"navigatorObject_firstChild": ("br(thaiBraille):F15"),
			"navigatorObject_moveFocus": ("br(thaiBraille):F16"),
			"navigatorObject_currentDimensions": ("br(thaiBraille):F17"),

			"kb:alt": ("br(thaiBraille):S82"),
			"kb:alt+tab": ("br(thaiBraille):S1E"),
			"kb:backspace": ("br(thaiBraille):S40"),
			"kb:control": ("br(thaiBraille):SC0"),
			"kb:control+end": ("br(thaiBraille):S38"), 
			"kb:control+home": ("br(thaiBraille):S07"),
			"kb:control+leftArrow": ("br(thaiBraille):F23"), 
			"kb:control+rightArrow": ("br(thaiBraille):F24"),
			"kb:delete": ("br(thaiBraille):S19"),			
			"kb:end": ("br(thaiBraille):S30"),
			"kb:enter": ("br(thaiBraille):S80"),
			"kb:escape": ("br(thaiBraille):S11"), 
			"kb:home": ("br(thaiBraille):S06"),
			"kb:pagedown": ("br(thaiBraille):S18"),
			"kb:pageup": ("br(thaiBraille):S03"),
			"kb:insert": ("br(thaiBraille):S88"),
			"kb:upArrow": ("br(thaiBraille):S01","br(thaiBraille):A02"),
			"kb:downArrow": ("br(thaiBraille):S08","br(thaiBraille):A04"), 
			"kb:leftArrow": ("br(thaiBraille):S04","br(thaiBraille):A01"),
			"kb:rightArrow": ("br(thaiBraille):S20","br(thaiBraille):A03"),
			"kb:shift": ("br(thaiBraille):S84"),
			"kb:shift+tab": ("br(thaiBraille):S05"),
			"kb:tab": ("br(thaiBraille):S28"),
			"kb:windows": ("br(thaiBraille):S81"),
			"kb:windows+b": ("br(thaiBraille):F39"),
			"kb:windows+d ": ("br(thaiBraille):C19")

		}
	})


class InputGestureKeys(braille.BrailleDisplayGesture):
	source = BrailleDisplayDriver.name

	def __init__(self, keys):
		super(InputGestureKeys, self).__init__()
		self.id =keys
		#keyNames[keys]

class InputGestureRouting(braille.BrailleDisplayGesture):
	source = BrailleDisplayDriver.name

	def __init__(self, index):
		super(InputGestureRouting, self).__init__()
		self.id = "routing"
		self.routingIndex = index-1
