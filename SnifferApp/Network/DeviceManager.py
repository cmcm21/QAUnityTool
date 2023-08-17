import socket
from threading import Thread
from Utils.Events import Event
from enum import Enum
from Command.CommandSignals import CommandSignal

buffer_size = 1024


class DeviceState(Enum):
    RECORDING = 0
    PLAYBACK = 1


class DeviceManager:

    def __init__(self, deviceSocket: socket.socket, address):
        print("client: " + str(address) + " connected")
        self.client = deviceSocket
        self.address = address
        self.deviceIp = address[0]
        self.listenThread = Thread(target=self._listenClientWorker)
        self.listeningClient = False
        self.msgReceivedEvent = Event()
        self.stateChangedEvent = Event()
        self.deviceDisconnectedEvent = Event()
        self.deviceState = DeviceState.RECORDING

    def _listenClientWorker(self):
        while self.listeningClient:
            try:
                message = self.client.recv(buffer_size).decode()
                self.msgReceivedEvent(message="Device[{}]: {}".format(self.deviceIp, message))
                print(message)
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
