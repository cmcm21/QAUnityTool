from PySide6 import QtWidgets


class DeviceWidget(QtWidgets.QWidget):
    def __init__(self):
        super().__init__()
        self.playBtn = QtWidgets.QPushButton("Play")
        self.stopBtn = QtWidgets.QPushButton("Stop")
        self.fileBtn = QtWidgets.QPushButton("Save")
        self.stateInfo = QtWidgets.QTextBrowser(self)
        self.container = QtWidgets.QHBoxLayout(self)
        self.buttonsLayout = QtWidgets.QVBoxLayout(self.container.widget())

        self.buttonsLayout.addWidget(self.playBtn)
        self.buttonsLayout.addWidget(self.stopBtn)
        self.buttonsLayout.addWidget(self.fileBtn)
        self.container.addLayout(self.buttonsLayout)
        self.container.addWidget(self.stateInfo)

    def __del__(self):
        return
