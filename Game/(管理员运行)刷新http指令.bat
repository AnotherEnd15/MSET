netsh http delete urlacl url=http://127.0.0.1:11000/
netsh http delete urlacl url=http://localhost:11000/
netsh http add urlacl url=http://*:11000/ user=Everyone

pause