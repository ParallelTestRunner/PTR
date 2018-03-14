@echo OFF & /C Title %title% - Started at: %time:~0,8% & nunit3-console.exe %test-container% /result:%result-file%;format=nunit2 %test-case-switch%--test:%test-case%,
