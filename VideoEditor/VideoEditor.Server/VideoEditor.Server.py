import sys
import src.server_main as server

def main():
    server.Start()

if __name__ == "__main__":
    sys.exit(int(main() or 0))