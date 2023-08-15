import socket
import threading

buffer_size = 1024


def process_client(client_socket: socket.socket, address):
    print("client: " + str(address) + " connected")
    while True:
        message = client_socket.recv(buffer_size).decode()
        print(message)


class SocketServer:
    def __init__(self):
        self.server = None
        self.socket = socket.socket()
        self.port = 8080
        self.maxConnections = 999
        self.hostname = socket.gethostname()
        self.Ip = socket.gethostbyname(self.hostname)
        self.clients_threads = []
        address = (self.Ip, self.port)
        self.socket.bind(address)
        self.socket.listen()
        print("Server started at: " + self.Ip + " on port: " + str(self.port))
        while True:
            (client_socket, address) = self.socket.accept()
            client_thread = threading.Thread(target=process_client, args=(client_socket, address,), daemon=True)
            client_thread.start()

            self.clients_threads.append(client_thread)

    def __del__(self):
        if self.server != None:
            self.server.close()
