# Docker
## Quickstart local app development

To quickly start a local Standalone development environment for app testing purposes you can use of the docker-compose file:
```bash
# Solution root
cd docker
docker-compose up --build
``` 
**Please be aware of the 'quickstart development'-only semi-secrets in `docker/**/*`, the Docker configuration has no intentions to be used in publicly available testing, acceptation or production environments.**
