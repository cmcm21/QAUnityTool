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
        self.streamingScrollArea =  self._initScrollArea()
        self.container = QtWidgets.QHBoxLayout()
        self.buttonsLayout = self._setButtonsLayout()
        self.container.addLayout(self.buttonsLayout)
        self.container.addWidget(self.streamingScrollArea)
        self.setLayout(self.container)
        self._disableAllButtons()

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

        self.setDefaultButtons()
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
        scrollArea.setVerticalScrollBarPolicy(QtCore.Qt.ScrollBarPolicy.ScrollBarAlwaysOn)
        scrollArea.setHorizontalScrollBarPolicy(QtCore.Qt.ScrollBarPolicy.ScrollBarAlwaysOff)
        scrollArea.setWidgetResizable(True)

        return scrollArea

    def _disableAllButtons(self):
        self.recordBtn.setEnabled(False)
        self.stopBtn.setEnabled(False)
        self.replayBtn.setEnabled(False)
        self.stopReplayBtn.setEnabled(False)
        self.loadFileBtn.setEnabled(False)

    def setDefaultButtons(self):
        self.recordBtn.setEnabled(True)
        self.loadFileBtn.setEnabled(True)
        self.stopBtn.setEnabled(True)
        self.replayBtn.setEnabled(False)
        self.stopReplayBtn.setEnabled(False)

    @QtCore.Slot()
    def onRecordBtnClicked(self):
        self.stopBtn.setEnabled(True)
        self.loadFileBtn.setEnabled(False)
        self.replayBtn.setEnabled(False)
        self.stopReplayBtn.setEnabled(False)

    @QtCore.Slot()
    def onStopBtnClicked(self):
        self.replayBtn.setEnabled(True)
        self.loadFileBtn.setEnabled(True)

    @QtCore.Slot()
    def onLoadBtnClicked(self):
        self.replayBtn.setEnabled(True)

    @QtCore.Slot()
    def onReplayBtnClicked(self):
        self.stopReplayBtn.setEnabled(True)

    def _setButtonsLayout(self) -> QtWidgets.QLayout:
        buttonsLayout = QtWidgets.QVBoxLayout()
        buttonsLayout.addWidget(self.recordBtn)
        buttonsLayout.addWidget(self.stopBtn)
        buttonsLayout.addWidget(self.loadFileBtn)
        buttonsLayout.addWidget(self.replayBtn)
        buttonsLayout.addWidget(self.stopReplayBtn)

        buttonsLayout.setContentsMargins(50, 10, 50, 10)
        return buttonsLayout

    def noDevices(self):
        for key, value in self.deviceScreensRef.items():
            value.showDefaultImage()

    def createDeviceScreen(self, device: DeviceClient):
        if self.screensUsed < MAX_DEVICES:
            screenDevice: DeviceScreen = self.screens[self.screensUsed]
            self.deviceScreensRef[device.id] = screenDevice
            screenDevice.setDevice(device)
            self.screensUsed += 1

    def deviceSelected(self, devices: list[DeviceClient]):
        self.setDefaultButtons()
        for device in devices:
            if device.id in self.deviceScreensRef:
                self.deviceScreensRef[device.id].show()

    def streamingClientConnected(self, client: StreamingClient, helper: StreamingVideoHelper):
        if client.id in self.deviceScreensRef:
            self.deviceScreensRef[client.id].setHelper(helper)

    def resetStreaming(self, address: str):
        if address in self.deviceScreensRef:
            self.deviceScreensRef[address].showDefaultImage()

    def __del__(self):
        return
