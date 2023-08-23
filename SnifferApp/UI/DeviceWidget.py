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
        self.stopBtn.setEnabled(False)
        self.saveFileBtn.setEnabled(False)
        self.replayBtn.setEnabled(False)
        self.stopReplayBtn.setEnabled(False)

    @QtCore.Slot()
    def onRecordBtnClicked(self):
        self.stopBtn.setEnabled(True)
        self.saveFileBtn.setEnabled(False)
        self.loadFileBtn.setEnabled(False)
        self.replayBtn.setEnabled(False)
        self.stopReplayBtn.setEnabled(False)

    @QtCore.Slot()
    def onStopBtnClicked(self):
        self.saveFileBtn.setEnabled(True)
        self.replayBtn.setEnabled(True)
        self.loadFileBtn.setEnabled(True)

    @QtCore.Slot()
    def onLoadBtnClicked(self):
        self.replayBtn.setEnabled(True)

    @QtCore.Slot()
    def onReplayBtnClicked(self):
        self.stopReplayBtn.setEnabled(True)

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
