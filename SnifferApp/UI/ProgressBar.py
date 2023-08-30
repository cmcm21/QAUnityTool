import time
from PySide6 import QtWidgets


class ProgressBar(QtWidgets.QWidget):
    def __init__(self):
        super().__init__()
        self.progressLayout = QtWidgets.QVBoxLayout(self)
        self.progressBar = QtWidgets.QProgressBar(self.progressLayout.widget())
        self.progressLabel = QtWidgets.QLabel()

        self.progressLayout.addWidget(self.progressLabel)
        self.progressLayout.addWidget(self.progressBar)
        self.max = 0

    def init(self, value: int, name: str):
        self.show()
        self.max = value
        self.progressLabel.setText(name)
        self.progressBar.setMaximum(value)

    def restore(self):
        self.max = 0
        self.progressLabel.setText("")
        self.progressBar.reset()

    def updateBar(self, newValue: int):
        value = self.progressBar.value() + newValue
        self.progressBar.setValue(value)

        if value >= self.max:
            self.progressBar.setMaximum(self.max)

    def hideBar(self):
        time.sleep(1)
        self.restore()
        self.hide()

