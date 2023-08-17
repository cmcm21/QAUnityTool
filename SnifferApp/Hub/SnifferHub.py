from Command.Command import *
from Network.ServerManager import ServerManager
from Network.DeviceManager import DeviceManager
from UI.UIManager import UIManager
from PySide6.QtWidgets import QPushButton
from PySide6 import QtCore


class SnifferHub:

    def __init__(self):
        self.uiManager = UIManager()
        self.serverManager = ServerManager()
        self.commandHistory = CommandHistory()
        self._connectEvents()
        self._initCommands()

    def _connectEvents(self):
        self.serverManager.newDeviceConnectedEvent += self._onNewDeviceAdded
        self.serverManager.serverInitEvent += self._onServerStarted
        self.uiManager.deviceWidget.deviceSelectedEvent += \
            lambda *ags, **kwargs: self.serverManager.setDeviceSelected(kwargs["device"])

    def _onServerStarted(self, *args, **kwargs):
        self.uiManager.uiLogger.appendText(kwargs["message"])
        return

    def _onNewDeviceAdded(self, *args, **kwargs):
        device: DeviceManager = kwargs["device"]
        device.msgReceivedEvent += self._onDeviceReceivedMessage
        device.stateChangedEvent += lambda: self.uiManager.deviceWidget.setWidgetState()
        device.deviceDisconnectedEvent += \
            lambda *args, **kwargs: self.uiManager.serverWidget.removeDevice(kwargs["device"])

        self.uiManager.serverWidget.addDevice(device)
        self.uiManager.deviceWidget.deviceSelected(device)
        return

    def _onDeviceReceivedMessage(self, *args, **kwargs):
        self.uiManager.uiLogger.appendText(kwargs["message"])
        return

    def _initCommands(self):
        initServerCommand = InitServerCommand(self, self.serverManager)
        self._connectCommandEventToLogger(initServerCommand)

        recordCommand = RecordCommand(self, self.serverManager)
        self._connectCommandEventToLogger(recordCommand)

        stopCommand = StopCommand(self, self.serverManager)
        self._connectCommandEventToLogger(stopCommand)

        replayCommand = ReplayCommand(self, self.serverManager)
        self._connectCommandEventToLogger(replayCommand)

        stopReplay = StopReplayCommand(self, self.serverManager)
        self._connectCommandEventToLogger(stopReplay)

        self._setButtonCommand(self.uiManager.serverWidget.initServerButton, initServerCommand)
        self._setButtonCommand(self.uiManager.deviceWidget.recordBtn, recordCommand)
        self._setButtonCommand(self.uiManager.deviceWidget.stopBtn, stopCommand)
        self._setButtonCommand(self.uiManager.deviceWidget.replayBtn, replayCommand)
        self._setButtonCommand(self.uiManager.deviceWidget.stopReplayBtn, stopReplay)

    def _connectCommandEventToLogger(self, command: Command):
        command.onCommandExecutedEvent += \
            lambda *args, **kwargs: self.uiManager.uiLogger.appendText(kwargs["message"])

    def app(self):
        self.uiManager.execute()

    @QtCore.Slot()
    def executeCommand(self, command: Command):
        if command.execute():
            self.commandHistory.push(command)

    def _setButtonCommand(self, button: QPushButton, command: Command):
        button.clicked.connect(lambda: self.executeCommand(command))

    def __del__(self):
        self.serverManager.newDeviceConnectedEvent -= self._onNewDeviceAdded
        self.serverManager.serverInitEvent -= self._onServerStarted
