[![GitHub Issues](https://img.shields.io/github/issues/obaki102/open-blazorui)](https://github.com/obaki102/open-blazorui/issues)
# open-blazorui
open-blazorui is a simple UI for your local LLMs. 
Drawing inspiration from the fantastic -  [open-webui](https://github.com/open-webui/open-webui).

![App Screenshot](https://github.com/obaki102/open-blazorui/blob/master/docs/Sample.gif)

# Running open-blazorui with Docker

This guide explains how to build and run the Media Toolkit Docker image.

## Prerequisites

Before you begin, ensure you have the following installed:
- Docker: [Install Docker](https://docs.docker.com/get-docker/)

## Step 1: Build the Docker Image

To build the Docker image, use the following command:
- (Note: Please run the command in the root folder of the project.)
```bash
docker build -f src/Open.Blazor.Ui/Dockerfile -t blazor-ui .
```

## Step 2: Run the Docker Container
Once the image is built, you can run the Docker container with the following command:

```bash
docker run -p 8081:8080 -d blazor-ui
```

## Step 3: Access open-blazorui
open-blazorui is now running inside the Docker container. You can access it by navigating to http://localhost:8081/ in your web browser.

## Step 4: Run Docker Compose (Optional)
If you prefer to use Docker Compose for managing your Docker containers, you can use the provided Docker Compose configuration file.

- Locate the `docker-compose-local.yml` file in the root folder of your project.

- Run the following command to start the containers defined in the Docker Compose file:
### local
```bash
docker-compose -f docker-compose-local.yml up
```

### docker hub
```bash
docker-compose -f docker-compose.yml up
```
