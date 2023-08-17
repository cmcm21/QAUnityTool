from typing import Any
from PySide6 import QtWidgets
from Network.DeviceManager import DeviceManager, DeviceState
from Utils.Events import Event


class DeviceWidget(QtWidgets.QWidget):
    def __init__(self):
        super().__init__()
        self.currentDevice: DeviceManager = Any
        self._initButtons()

        self.stateInfo = QtWidgets.QTextBrowser(self)
        self.container = QtWidgets.QHBoxLayout(self)

        self._setButtonsLayout()

        self.container.addLayout(self.buttonsLayout)
        self.container.addWidget(self.stateInfo)
        self.deviceSelectedEvent = Event()

    def _initButtons(self):
        self.recordBtn = QtWidgets.QPushButton("Record")
        self.stopBtn = QtWidgets.QPushButton("Stop")
        self.saveFileBtn = QtWidgets.QPushButton("Save")
        self.replayBtn = QtWidgets.QPushButton("Replay")
        self.stopReplayBtn = QtWidgets.QPushButton("Stop Replay")
        self.loadFileBtn = QtWidgets.QPushButton("Load")

    def _setButtonsLayout(self):
        self.buttonsLayout = QtWidgets.QVBoxLayout(self.container.widget())
        self.buttonsLayout.addWidget(self.recordBtn)
        self.buttonsLayout.addWidget(self.stopBtn)
        self.buttonsLayout.addWidget(self.saveFileBtn)
        self.buttonsLayout.addWidget(self.loadFileBtn)
        self.buttonsLayout.addWidget(self.replayBtn)
        self.buttonsLayout.addWidget(self.stopReplayBtn)

    def deviceSelected(self, device: DeviceManager):
        self.show()
        self.currentDevice = device
        self.deviceSelectedEvent(device=self.currentDevice)

    def setWidgetState(self):
        if self.currentDevice.deviceState == DeviceState.RECORDING:
            self._setRecState()
        else:
            self._setPlaybackState()

    def _setRecState(self):
        self.replayBtn.hide()
        self.stopReplayBtn.hide()
        self.loadFileBtn.hide()

        self.recordBtn.show()
        self.stopBtn.show()
        self.saveFileBtn.show()

    def _setPlaybackState(self):
        self.recordBtn.hide()
        self.stopBtn.hide()
        self.saveFileBtn.hide()

        self.replayBtn.show()
        self.stopReplayBtn.show()
        self.loadFileBtn.show()

    def __del__(self):
        return
