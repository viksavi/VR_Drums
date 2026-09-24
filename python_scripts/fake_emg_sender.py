import socket
import time
import random

UDP_IP = "127.0.0.1"
UDP_PORT = 5555          # must match the port set in Unity's UDPForceReceiver

MIN_DELAY_SECONDS = 5    # shortest gap between fake "hits"
MAX_DELAY_SECONDS = 15   # longest gap between fake "hits"
MIN_FORCE_PERCENT = 25   # weakest fake hit
MAX_FORCE_PERCENT = 100  # strongest fake hit

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

print(f"Sending sporadic fake force values to {UDP_IP}:{UDP_PORT}, "
      f"every {MIN_DELAY_SECONDS}-{MAX_DELAY_SECONDS}s. Ctrl+C to stop.")

try:
    while True:
        delay = random.uniform(MIN_DELAY_SECONDS, MAX_DELAY_SECONDS)
        time.sleep(delay)

        force = random.uniform(MIN_FORCE_PERCENT, MAX_FORCE_PERCENT)
        message = f"{force:.2f}"
        sock.sendto(message.encode("utf-8"), (UDP_IP, UDP_PORT))
        print(f"Sent: {message}% (after a {delay:.1f}s gap)")
except KeyboardInterrupt:
    print("\nStopped.")
finally:
    sock.close()