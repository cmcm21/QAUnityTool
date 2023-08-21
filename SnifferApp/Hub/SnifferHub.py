from Command.Command import *
from Network.ServerManager import ServerManager
from Network.DeviceManager import DeviceManager
from UI.UIManager import UIManager
from PySide6.QtWidgets import QPushButton
from PySide6 import QtCore
from enum import  Enum


class ApplicationState(Enum):
    PROCESSING = 0
    IDLE = 1


class SnifferHub:

    def __init__(self):
        self.uiManager = UIManager()
        self.serverManager = ServerManager()
        self.commandHistory = CommandHistory()
        self.appSate = ApplicationState.IDLE
        self._connectEvents()
        self._initCommands()

    def _connectEvents(self):
        self.serverManager.newDeviceConnectedEvent += self._onNewDeviceAdded
        self.serverManager.serverInitEvent += self._onServerStarted
        self.uiManager.deviceWidget.deviceSelectedEvent += self._onDeviceSelected
        self.serverManager.fileServer.fileReceiveStartedEvent += self._onSavingStarted
        self.serverManager.fileServer.fileReceiveFinishedEvent += self._onSavingEnded
        self.serverManager.fileServer.fileSendingStartedEvent += self._onLoadingStarted
        self.serverManager.fileServer.fileSendingEndedEvent += self._onSavingEnded

    def _onNewDeviceAdded(self, *args, **kwargs):
        device: DeviceManager = kwargs["device"]
        device.msgReceivedEvent += self._onDeviceReceivedMessage
        device.stateChangedEvent += lambda: self.uiManager.deviceWidget.setWidgetState()
        device.deviceDisconnectedEvent += \
            lambda *args, **kwargs: self.uiManager.serverWidget.removeDevice(kwargs["device"])

        self.uiManager.serverWidget.addDevice(device)
        self.uiManager.deviceWidget.deviceSelected(device)
        return

    def _onDeviceSelected(self, *args, **kwargs):
        self.serverManager.setDeviceSelected(kwargs["device"])

    def _onServerStarted(self, *args, **kwargs):
        self.uiManager.uiLogger.appendText(kwargs["message"])
        return

    def _onDeviceReceivedMessage(self, *args, **kwargs):
        self.uiManager.uiLogger.appendText(kwargs["message"])
        return

    def _onSavingStarted(self, *args, **kwargs):
        self.appSate = ApplicationState.PROCESSING
        self.uiManager.uiLogger.appendText(f"File {kwargs['file']} of size {kwargs['size']} is saving...")
        return

    def _onSavingEnded(self, *args, **kwargs):
        self.appSate = ApplicationState.IDLE
        self.uiManager.uiLogger.appendText(f"File {kwargs['file']} saving completed")
        return

    def _onLoadingStarted(self, *args, **kwargs):
        self.appSate = ApplicationState.PROCESSING
        self.uiManager.uiLogger.appendText(f"File {kwargs['file']} is loading in device: {kwargs['address']}...")

    def _onLoadingEnded(self, *args, **kwargs):
        self.appSate = ApplicationState.PROCESSING
        self.uiManager.uiLogger.appendText(f"File {kwargs['file']} loading completed in device {kwargs['address']}")

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

        saveCommand = SaveFileCommand(self, self.serverManager)
        self._connectCommandEventToLogger(saveCommand)

        loadCommand = LoadFileCommand(self, self.serverManager)
        self._connectCommandEventToLogger(loadCommand)

        self._setButtonCommand(self.uiManager.serverWidget.initServerButton, initServerCommand)
        self._setButtonCommand(self.uiManager.deviceWidget.recordBtn, recordCommand)
        self._setButtonCommand(self.uiManager.deviceWidget.stopBtn, stopCommand)
        self._setButtonCommand(self.uiManager.deviceWidget.replayBtn, replayCommand)
        self._setButtonCommand(self.uiManager.deviceWidget.stopReplayBtn, stopReplay)
        self._setButtonCommand(self.uiManager.deviceWidget.saveFileBtn, saveCommand)
        self._setButtonCommand(self.uiManager.deviceWidget.loadFileBtn, loadCommand)

    def _connectCommandEventToLogger(self, command: Command):
        command.onCommandExecutedEvent += \
            lambda *args, **kwargs: self.uiManager.uiLogger.appendText(kwargs["message"])

    def app(self):
        self.uiManager.execute()

    @QtCore.Slot()
    def executeCommand(self, command: Command):
        if self.appSate == ApplicationState.PROCESSING:
            return

        if command.execute():
            self.commandHistory.push(command)

    def _setButtonCommand(self, button: QPushButton, command: Command):
        button.clicked.connect(lambda: self.executeCommand(command))

    def __del__(self):
        self.serverManager.newDeviceConnectedEvent -= self._onNewDeviceAdded
        self.serverManager.serverInitEvent -= self._onServerStarted
        self.uiManager.deviceWidget.deviceSelectedEvent -= self._onDeviceSelected
