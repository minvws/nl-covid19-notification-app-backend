# Docker
## Quickstart local app development

To quickly start a local development environment for app testing purposes you can use of the docker-compose file:
```bash
# Solution root
cd docker
docker-compose up
``` 

To rebuild the image after a `git pull` run `docker-compose up --build`.

This will run the database container and DbBuilder to create the databases.

## Running command line tools
To run command line tools, use `docker exec` to login to one of the running containers, e.g. `docker exec -ti database /bin/sh`.

**Please be aware of the 'quickstart development'-only semi-secrets in `docker/**/*`, the Docker configuration has no intentions to be used in publicly available testing, acceptation or production environments.**
