from Commands.Command import *
from Network.Servers.ServerManager import ServerManager
from Network.Clients.DeviceClient import DeviceClient
from UI.UIManager import UIManager
from PySide6.QtWidgets import QPushButton
from PySide6 import QtCore
from Hub.SnifferState import  SnifferState
from Utils.StreamingVideoHelper import StreamingVideoHelper


class SnifferHub:

    def __init__(self):
        self.uiManager = UIManager()
        self.serverManager = ServerManager()
        self.commandHistory = CommandHistory()
        self.appSate = SnifferState.IDLE

        self._connectEvents()
        self._connectQSignals()
        self._initCommands()

    def app(self):
        self.uiManager.execute()

    def _connectEvents(self):
        self.uiManager.onApplicationQuitEvent += self._onApplicationQuit
        self.serverManager.newDeviceConnectedEvent += self._onNewDeviceAdded
        self.serverManager.serverInitEvent += self._onServerStarted
        self.serverManager.NoMoreDevicesConnectedEvent += self._onNoMoreDevices
        self.uiManager.serverWidget.devicesSelectedChanged += self._onDevicesSelectedChanged
        self.serverManager.fileServer.fileReceiveStartedEvent += self._onSavingStarted
        self.serverManager.fileServer.fileReceiveFinishedEvent += self._onSavingEnded
        self.serverManager.fileServer.fileSendingStartedEvent += self._onLoadingStarted
        self.serverManager.fileServer.fileSendingFinishEvent += self._onLoadingEnded
        self.serverManager.streamingServer.onNewStreamingClientConnected += self._onStreamingClientConnected

    def _connectQSignals(self):
        self.serverManager.streamingServer.qSignal.connect(
            self.uiManager.deviceWidget.setStreamingImage
        )

        self.serverManager.fileServer.qFileTransferStarSignal.connect(
            self.uiManager.fileTransferringProgressbar.init
        )

        self.serverManager.fileServer.qFileTransferUpdateSignal.connect(
            self.uiManager.fileTransferringProgressbar.updateBar
        )

        self.serverManager.fileServer.qFileTransferEndSignal.connect(
            self.uiManager.fileTransferringProgressbar.endProgressBar
        )

    def _onApplicationQuit(self, *args, **kwargs):
        del self.uiManager
        del self.serverManager
        del self

    def _onNewDeviceAdded(self, *args, **kwargs):
        device: DeviceClient = kwargs["device"]
        device.msgReceivedEvent += self._onDeviceReceivedMessage
        device.deviceDisconnectedEvent += self._onDeviceDisconnected
        device.deviceDisconnectedEvent += self.uiManager.deviceWidget.onDeviceDisconnected
        device.replayFinished += lambda *args, **kwargs: self.uiManager.deviceWidget.resetStreaming(device.id)
        device.hostnameUpdatedEvent += self.uiManager.deviceWidget.updateDeviceScreen
        device.hostnameUpdatedEvent += self.uiManager.serverWidget.updateDevice

        self.uiManager.deviceWidget.createDeviceScreen(device)
        self.uiManager.serverWidget.addDevice(device)

    def _onDeviceReceivedMessage(self, *args, **kwargs):
        self.uiManager.uiLogger.appendText(kwargs["message"])

    def _onDeviceDisconnected(self, *args, **kwargs):
        device = kwargs['device']
        self.uiManager.serverWidget.removeDevice(device)

    def _onServerStarted(self, *args, **kwargs):
        self.uiManager.uiLogger.appendText(kwargs["message"])

    def _onNoMoreDevices(self, *args, **kwargs):
        self.uiManager.deviceWidget.noDevices()
        self.uiManager.uiLogger.appendText("No Device Selected")

    def _onDevicesSelectedChanged(self, *args, **kwargs):
        self.serverManager.setDeviceSelected(kwargs["devices"])

    def _onSavingStarted(self, *args, **kwargs):
        self.appSate = SnifferState.PROCESSING
        self.uiManager.uiLogger.appendText(f"File {kwargs['file']} of size {kwargs['size']} is saving...")

    def _onSavingEnded(self, *args, **kwargs):
        self.appSate = SnifferState.IDLE
        self.uiManager.uiLogger.appendText(f"File {kwargs['file']} saving completed")

    def _onLoadingStarted(self, *args, **kwargs):
        self.appSate = SnifferState.PROCESSING
        self.uiManager.uiLogger.appendText(f"File {kwargs['file']} is loading in device: {kwargs['address']}...")

    def _onLoadingEnded(self, *args, **kwargs):
        self.appSate = SnifferState.IDLE
        self.uiManager.uiLogger.appendText(f"File {kwargs['file']} loading completed in device {kwargs['address']}")

    def _onStreamingClientConnected(self, *args, **kwargs):
        if 'client' in kwargs and 'helper' in kwargs:
            client = kwargs['client']
            helper: StreamingVideoHelper = kwargs['helper']
            helper.fileSavedEvent += self._onStreamingSaved
            self.uiManager.deviceWidget.streamingClientConnected(client, helper)

    def _onStreamingSaved(self, *args, **kwargs):
        fileName = kwargs['fileName']
        self.uiManager.uiLogger.appendText(f"Streaming Video : {fileName} was saved correctly")

    def _initCommands(self):
        initServerCommand = InitServerCommand(self, self.serverManager)
        recordCommand = RecordCommand(self, self.serverManager)
        stopCommand = StopCommand(self, self.serverManager)
        replayCommand = ReplayCommand(self, self.serverManager)
        stopReplayCommand = StopReplayCommand(self, self.serverManager)
        replayOneStepCommand = ReplayOneStepCommand(self, self.serverManager)
        loadCommand = LoadFileCommand(self, self.serverManager)
        autoSaveCommand = AutoSaveCommand(self, self.serverManager)

        self._connectCommandEventToLogger(
            initServerCommand,
            recordCommand,
            stopCommand,
            replayCommand,
            replayOneStepCommand,
            stopReplayCommand,
            loadCommand,
            autoSaveCommand
        )

        self._setButtonCommand(self.uiManager.serverWidget.initServerButton, initServerCommand)
        self._setButtonCommand(self.uiManager.deviceWidget.recordBtn, recordCommand)
        self._setButtonCommand(self.uiManager.deviceWidget.stopBtn, stopCommand, autoSaveCommand)
        self._setButtonCommand(self.uiManager.deviceWidget.replayBtn, replayCommand)
        self._setButtonCommand(self.uiManager.deviceWidget.stopReplayBtn, stopReplayCommand)
        self._setButtonCommand(self.uiManager.deviceWidget.loadFileBtn, loadCommand)
        self._setButtonCommand(self.uiManager.deviceWidget.replayOneStepBtn, replayOneStepCommand)

    def _connectCommandEventToLogger(self, *args):
        for command in args:
            command.onCommandExecutedEvent += \
                lambda *args, **kwargs: self.uiManager.uiLogger.appendText(kwargs["message"])

    def _setButtonCommand(self, button: QPushButton, *args):
        for command in args:
            button.clicked.connect(self.executeCommandWrapper(command))

    @QtCore.Slot()
    def executeCommandWrapper(self, command: Command):
        def executeCommand():
            if self.appSate == SnifferState.PROCESSING:
                self.uiManager.uiLogger.appendText(
                    f"Application is working, cannot handle command: {command.__class__}")
                return

            if command.execute():
                self.commandHistory.push(command)

        return executeCommand

    def __del__(self):
        self.serverManager.newDeviceConnectedEvent -= self._onNewDeviceAdded
        self.serverManager.serverInitEvent -= self._onServerStarted
        self.serverManager.NoMoreDevicesConnectedEvent -= self._onNoMoreDevices
        self.uiManager.serverWidget.devicesSelectedChanged -= self._onDevicesSelectedChanged
        self.serverManager.fileServer.fileReceiveStartedEvent -= self._onSavingStarted
        self.serverManager.fileServer.fileReceiveFinishedEvent -= self._onSavingEnded
        self.serverManager.fileServer.fileSendingStartedEvent -= self._onLoadingStarted
        self.serverManager.fileServer.fileSendingFinishEvent -= self._onLoadingEnded
        for helper in self.serverManager.streamingServer.helpers:
            helper -= self._onStreamingSaved
