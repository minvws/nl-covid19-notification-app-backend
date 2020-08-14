# Docker
## Quickstart local app development

To quickly start a local development environment for app testing purposes you can use of the docker-compose file:
```bash
# Solution root
cd docker
docker-compose up --build
``` 

This will run the database, content api, mobile api and jobs api. Endpoints can be found here:

* Mobile API http://localhost:5004/
* Content API http://localhost:5005/
* BatchJobs API http://localhost:5006/

Swagger is available on all endpoints by appending `swagger` to the urls listed.

**Please be aware of the 'quickstart development'-only semi-secrets in `docker/**/*`, the Docker configuration has no intentions to be used in publicly available testing, acceptation or production environments.**
