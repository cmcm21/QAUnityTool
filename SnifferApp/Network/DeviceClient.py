import os.path
import socket
from threading import Thread
from Utils.Events import Event
from enum import Enum
from Command.CommandSignals import CommandSignal

BUFFER_SIZE = 1024
FILE_BUFFER_SIZE = 4096


class DeviceState(Enum):
    IDLE = -1
    RECORDING = 0
    PLAYBACK = 1


class DeviceClient:

    def __init__(self, deviceSocket: socket.socket, address):
        self.deviceState = DeviceState.IDLE
        print("client: " + str(address) + " connected")
        self.client = deviceSocket
        self.address = address
        self.deviceIp = address[0]
        self.listenThread = Thread(target=self._listenClientWorker,daemon=True)
        self.listeningClient = False
        self.msgReceivedEvent = Event()
        self.stateChangedEvent = Event()
        self.deviceDisconnectedEvent = Event()

    def _listenClientWorker(self):
        while self.listeningClient:
            try:
                message = self.client.recv(BUFFER_SIZE)
                if message == b'':
                    self._handleDeviceDisconnection()
                else:
                    self.msgReceivedEvent(message="Device[{}]: {}".format(self.deviceIp, message.decode()))
                    self._decodeClientMessage(message.decode())
            except ConnectionError:
                self._handleDeviceDisconnection()

    def _handleDeviceDisconnection(self):
        print(f"Device client {self.address} disconnected")
        self.deviceDisconnectedEvent(device=self)
        self.close()
        return

    def _decodeClientMessage(self, message: str):
        if message == CommandSignal.CHANGE_STATE.value:
            self.changeState()

    def sendSignalToDevice(self, signal: CommandSignal):
        try:
            send = self.client.send(signal.value.encode())
            if send == 0:
                self._handleDeviceDisconnection()
        except ConnectionError:
            self._handleDeviceDisconnection()

    def starListening(self):
        self.listeningClient = True
        self.listenThread.start()

    def changeState(self):
        newState = DeviceState.PLAYBACK if self.deviceState == DeviceState.RECORDING else DeviceState.RECORDING
        self.deviceState = newState
        self.stateChangedEvent(state=self.deviceState)

    def close(self):
        self.listeningClient = False

        if self.listenThread.is_alive():
            try:
                self.listenThread.join()
            except RuntimeError:
                print(f"device {self.address} listening thread couldn't join ")

        self.client.close()
