from pathlib import Path
import hashlib,struct,sys
p=Path(sys.argv[1]);data=p.read_bytes()
assert hashlib.sha256(data).hexdigest() == "58012e57cebc499454812ba7ed96b1289db01e520963b4fc364edb41c322b2a8"
pe=struct.unpack_from('<I',data,60)[0];assert struct.unpack_from('<H',data,pe+4)[0]==0x14c
assert struct.unpack_from('<I',data,pe+24+56)[0]==0x15bf000
# This client uses equal file and section RVAs for the executable code section.
for offset,expected in [(0x88d291,'8b8ea0000000e844994700'),(0x102e30,'a1ec119101c3'),(0xd06be0,'568bf18b06ff90a4000000'),(0x985060,'558bec8b4508'),(0x9848d0,'558bec6aff'),(0x984b4b,'e8a09a6eff'),(0x5797c1,'8a450c84c00f84e6000000e8ffb9aaff'),(0x2cc1a5,'8a45e484c074058b4dcc890b8bb674020000'),(0x8a9e6e,'e82d23eaff84c07458a058889001')]:
 assert data[offset:offset+len(bytes.fromhex(expected))]==bytes.fromhex(expected)
print('PASS: exact supported executable hash, PE architecture/image size, and all native hook signatures.')
