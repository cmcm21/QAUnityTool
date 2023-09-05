import time
from PySide6 import QtWidgets, QtCore, QtGui
from Utils.StreamingVideoHelper import StreamingVideoHelper
from UI import UIManager


class ProgressBar(QtWidgets.QWidget):
    def __init__(self, size=None):
        super().__init__()
        self.progressLayout = QtWidgets.QVBoxLayout()
        self.progressBar = QtWidgets.QProgressBar()
        self.progressLabel = QtWidgets.QLabel()
        if size is not None:
            UIManager.setWidgetSize(size, self.progressBar, self.progressLabel)
        else:
            self.progressBar.setMaximumHeight(30)
            self.progressBar.setMinimumWidth(QtGui.QGuiApplication.primaryScreen().size().width())

        self.progressLayout.addWidget(self.progressLabel, alignment=QtCore.Qt.AlignmentFlag.AlignHCenter)
        self.progressLayout.addWidget(self.progressBar, alignment=QtCore.Qt.AlignmentFlag.AlignBottom)
        self.progressLayout.setSpacing(0)
        self.progressLayout.setContentsMargins(0, 0, 0, 0)
        self.max = 0
        self.setLayout(self.progressLayout)

    def setStreamingHelper(self, streamingHelper: StreamingVideoHelper):
        self._connectWithHelper(streamingHelper)

    def _connectWithHelper(self, helper: StreamingVideoHelper):
        helper.qRenderingStartSignal.connect(self.init)
        helper.qRenderingEndSignal.connect(self.hide)
        helper.qFrameRenderedSignal.connect(self.updateBar)

    def init(self, value: int, name: str):
        self.setVisible(True)
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

    def endProgressBar(self):
        time.sleep(1)
        self.restore()
        self.setVisible(False)
