EXECUTABLE_NAME = ipk24chat-client

build:
	dotnet publish -r linux-x64 -p:PublishSingleFile=true
	mv bin/Release/net8.0/linux-x64/publish/IPK_Project1 .
	mv IPK_Project1 $(EXECUTABLE_NAME)

clean:
	dotnet clean
	rm -rf $(EXECUTABLE_NAME)

all: clean build

.PHONY: build clean
