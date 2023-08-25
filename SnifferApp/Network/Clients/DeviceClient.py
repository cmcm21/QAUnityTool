import socket
from Utils.Events import Event
from enum import Enum
from Command.CommandSignals import CommandSignal
from Network.GeneralSocket import GeneralSocket

BUFFER_SIZE = 1024


class DeviceState(Enum):
    IDLE = -1
    RECORDING = 0
    PLAYBACK = 1


class DeviceClient(GeneralSocket):

    def __init__(self, deviceSocket: socket.socket, address):
        super().__init__(deviceSocket, address)
        print("Device client: " + str(address) + " connected")
        self.deviceState = DeviceState.IDLE
        self.msgReceivedEvent = Event()
        self.stateChangedEvent = Event()
        self.deviceDisconnectedEvent = Event()

    def start(self):
        self.listeningSocket = True
        self.socketThread.start()

    def _socketWorker(self):
        while self.listeningSocket:
            try:
                message = self.socket.recv(BUFFER_SIZE)
                if message == b'':
                    self._handleDeviceDisconnection()
                else:
                    self.msgReceivedEvent(message="Device[{}]: {}".format(self.ip, message.decode()))
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
            send = self.socket.send(signal.value.encode())
            if send == 0:
                self._handleDeviceDisconnection()
        except ConnectionError:
            self._handleDeviceDisconnection()

    def changeState(self):
        newState = DeviceState.PLAYBACK if self.deviceState == DeviceState.RECORDING else DeviceState.RECORDING
        self.deviceState = newState
        self.stateChangedEvent(state=self.deviceState)
