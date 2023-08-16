from CommandPattern.Command import CommandHistory, InitServerCommand, PlayCommand
from Network.ServerManager import ServerManager
from Network.DeviceManager import DeviceManager
from UI.UIManager import UIManager
from PySide6.QtWidgets import QPushButton
from PySide6 import QtCore
from CommandPattern import Command

Command = {
    Command,
    CommandHistory,
    InitServerCommand,
    PlayCommand
}


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

    def _onServerStarted(self, *args, **kwargs):
        self.uiManager.uiLogger.appendText(kwargs["message"])
        return

    def _onNewDeviceAdded(self, *args, **kwargs):
        device: DeviceManager = kwargs["device"]
        device.msgReceivedEvent += self._onDeviceReceivedMessage
        self.uiManager.serverWidget.addDevice(device)
        return

    def _onDeviceReceivedMessage(self, *args, **kwargs):
        self.uiManager.uiLogger.appendText(kwargs["message"])
        return

    def _initCommands(self):
        self._setButtonCommand(
            self.uiManager.serverWidget.initServerButton,
            InitServerCommand(self, self.serverManager)
        )

    def app(self):
        self.uiManager.execute()

    @QtCore.Slot()
    def executeCommand(self, command: Command):
        if command.execute():
            self.commandHistory.push(command)

    def _setButtonCommand(self, button: QPushButton, command: Command):
        button.clicked.connect(lambda: self.executeCommand(command))
