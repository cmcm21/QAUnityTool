from typing import Any
from PySide6 import QtWidgets, QtCore
from Network.DeviceClient import DeviceClient, DeviceState
from Utils.Events import Event


class DeviceWidget(QtWidgets.QWidget):
    def __init__(self):
        super().__init__()
        self._initButtons()

        self.stateInfo = QtWidgets.QTextBrowser(self)
        self.container = QtWidgets.QHBoxLayout(self)

        self._setButtonsLayout()

        self.container.addLayout(self.buttonsLayout)
        self.container.addWidget(self.stateInfo)

    def _initButtons(self):
        self.recordBtn = QtWidgets.QPushButton("Record")
        self.stopBtn = QtWidgets.QPushButton("Stop")
        self.saveFileBtn = QtWidgets.QPushButton("Save")
        self.replayBtn = QtWidgets.QPushButton("Replay")
        self.stopReplayBtn = QtWidgets.QPushButton("Stop Replay")
        self.loadFileBtn = QtWidgets.QPushButton("Load")

        self._setDefaultButtons()

        self.recordBtn.clicked.connect(self.onRecordBtnClicked)
        self.stopBtn.clicked.connect(self.onStopBtnClicked)
        self.loadFileBtn.clicked.connect(self.onLoadBtnClicked)
        self.replayBtn.clicked.connect(self.onReplayBtnClicked)

    def _setDefaultButtons(self):
        self.stopBtn.hide()
        self.saveFileBtn.hide()
        self.replayBtn.hide()
        self.stopReplayBtn.hide()

    @QtCore.Slot()
    def onRecordBtnClicked(self):
        self.stopBtn.show()

    @QtCore.Slot()
    def onStopBtnClicked(self):
        self.saveFileBtn.show()
        self.replayBtn.show()

    @QtCore.Slot()
    def onLoadBtnClicked(self):
        self.replayBtn.show()

    @QtCore.Slot()
    def onReplayBtnClicked(self):
        self.stopReplayBtn.show()

    def _setButtonsLayout(self):
        self.buttonsLayout = QtWidgets.QVBoxLayout(self.container.widget())
        self.buttonsLayout.addWidget(self.recordBtn)
        self.buttonsLayout.addWidget(self.stopBtn)
        self.buttonsLayout.addWidget(self.saveFileBtn)
        self.buttonsLayout.addWidget(self.loadFileBtn)
        self.buttonsLayout.addWidget(self.replayBtn)
        self.buttonsLayout.addWidget(self.stopReplayBtn)

    def noDevices(self):
        self.hide()

    def deviceSelected(self, device: DeviceClient):
        self._setDefaultButtons()
        self.show()

    def __del__(self):
        return
