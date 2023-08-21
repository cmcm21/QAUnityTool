import os.path
import socket
from threading import Thread
from Utils.Events import Event
from enum import Enum
from Command.CommandSignals import CommandSignal

BUFFER_SIZE = 1024
FILE_BUFFER_SIZE = 4096


class DeviceState(Enum):
    IDLE =  -1
    RECORDING = 0
    PLAYBACK = 1


class DeviceManager:

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
                message = self.client.recv(BUFFER_SIZE).decode()
                self.msgReceivedEvent(message="Device[{}]: {}".format(self.deviceIp, message))
                self._decodeClientMessage(message)
            except ConnectionResetError:
                self.deviceDisconnectedEvent(device=self)

    def _decodeClientMessage(self, message: str):
        if message == CommandSignal.CHANGE_STATE.value:
            self.changeState()

    def sendSignalToDevice(self, signal: CommandSignal):
        self.client.send(signal.value.encode())

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
            self.listenThread.join()
