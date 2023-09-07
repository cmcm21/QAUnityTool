from PySide6 import QtWidgets, QtCore
from Network.Clients.DeviceClient import DeviceClient
from Network.Clients.StreamingClient import *
from PySide6.QtGui import QPixmap
from UI.DeviceScreen import DeviceScreen
from Utils.Settings import *
from UI import UIManager


class DeviceWidget(QtWidgets.QWidget):
    def __init__(self):
        super().__init__()
        self.deviceScreensRef: {str: DeviceScreen} = {}
        self.screens = []
        self.screensUsed = 0

        self._initButtons()
        self.streamingContainer = self._initStreamingScreens()
        self.streamingScrollArea = self._initScrollArea()
        self.container = QtWidgets.QHBoxLayout()
        self.buttonsLayout = self._setButtonsLayout()
        self.container.addLayout(self.buttonsLayout)
        self.container.addWidget(self.streamingScrollArea)
        self.setLayout(self.container)

    def setStreamingImage(self, pixmap: QPixmap, clientId: str):
        if clientId in self.deviceScreensRef:
            self.deviceScreensRef[clientId].setStreamingImage(pixmap)

    def _initButtons(self):
        buttonsSize = QtCore.QSize(90, 30)
        self.recordBtn = QtWidgets.QPushButton("Record")
        self.stopBtn = QtWidgets.QPushButton("Stop")
        self.replayBtn = QtWidgets.QPushButton("Replay")
        self.stopReplayBtn = QtWidgets.QPushButton("Stop Replay")
        self.loadFileBtn = QtWidgets.QPushButton("Load")
        UIManager.setWidgetSize(
            buttonsSize,
            self.replayBtn,
            self.stopBtn, self.replayBtn,
            self.stopReplayBtn,
            self.loadFileBtn
        )

        self.recordBtn.clicked.connect(self.onRecordBtnClicked)
        self.stopBtn.clicked.connect(self.onStopBtnClicked)
        self.loadFileBtn.clicked.connect(self.onLoadBtnClicked)
        self.replayBtn.clicked.connect(self.onReplayBtnClicked)

    def _initStreamingScreens(self) -> QtWidgets.QLayout:
        streamingContainer = QtWidgets.QGridLayout()
        row = 0
        column = 0
        for i in range(MAX_DEVICES):
            screenDevice = DeviceScreen()
            self.screens.append(screenDevice)
            streamingContainer.addWidget(screenDevice, row, column)
            column += 1
            if column > 1:
                row += 1
                column = 0

        streamingContainer.setSpacing(0)
        streamingContainer.setContentsMargins(0, 0, 0, 0)
        return streamingContainer

    def _initScrollArea(self) -> QtWidgets.QScrollArea:
        widget = QtWidgets.QWidget()
        scrollArea = QtWidgets.QScrollArea()
        scrollArea.setWidget(widget)
        widget.setLayout(self.streamingContainer)
        scrollArea.setVerticalScrollBarPolicy(QtCore.Qt.ScrollBarPolicy.ScrollBarAsNeeded)
        scrollArea.setHorizontalScrollBarPolicy(QtCore.Qt.ScrollBarPolicy.ScrollBarAlwaysOff)
        scrollArea.setWidgetResizable(True)

        return scrollArea

    @QtCore.Slot()
    def onRecordBtnClicked(self):
        pass

    @QtCore.Slot()
    def onStopBtnClicked(self):
        return

    @QtCore.Slot()
    def onLoadBtnClicked(self):
        return

    @QtCore.Slot()
    def onReplayBtnClicked(self):
        return

    def _setButtonsLayout(self) -> QtWidgets.QLayout:
        buttonsLayout = QtWidgets.QVBoxLayout()
        buttonsLayout.addWidget(self.recordBtn)
        buttonsLayout.addWidget(self.stopBtn)
        buttonsLayout.addWidget(self.loadFileBtn)
        buttonsLayout.addWidget(self.replayBtn)
        buttonsLayout.addWidget(self.stopReplayBtn)

        buttonsLayout.setContentsMargins(50, 10, 50, 10)
        return buttonsLayout

    def onDeviceDisconnected(self, *args, **kwargs):
        if self.screensUsed > 0:
            self.screensUsed -= 1

    def noDevices(self):
        for key, value in self.deviceScreensRef.items():
            value.reset()

    def createDeviceScreen(self, device: DeviceClient):
        if self.screensUsed < MAX_DEVICES:
            screenDevice: DeviceScreen = self.screens[self.screensUsed]
            self.deviceScreensRef[device.id] = screenDevice
            screenDevice.setDevice(device)
            self.screensUsed += 1

    def updateDeviceScreen(self, *args, **kwargs):
        if 'device' in kwargs:
            device: DeviceClient = kwargs['device']
            if device.id in self.deviceScreensRef:
                self.deviceScreensRef[device.id].setDevice(device)

    def deviceSelected(self, devices: list[DeviceClient]):
        for device in devices:
            if device.id in self.deviceScreensRef:
                self.deviceScreensRef[device.id].show()

    def streamingClientConnected(self, client: StreamingClient, helper: StreamingVideoHelper):
        if client.id in self.deviceScreensRef:
            self.deviceScreensRef[client.id].setHelper(helper)

    def resetStreaming(self, address: str):
        if address in self.deviceScreensRef:
            self.deviceScreensRef[address].reset()

    def __del__(self):
        return
