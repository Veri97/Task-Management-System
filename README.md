# Task Management System

The project represents a Task Management System with integration to RabbitMQ message broker. Here you will find the instructions on how to build and run the application

## Getting Started

IMPORTANT NOTE: It is a prerequisite to have Docker installed on your system in order to run the project.

- The Api depends on SQL Server database and RabbitMQ service which run on Docker. I have created a docker-compose file which contains both of these services.
  To run the docker-compose file, navigate at the root directory of the project where this file is located, and use the following command in the command line:

  ` docker-compose up `

- After containers are up and running, you can build and run the project (TasksManagement.Api) from your Visual Studio or Rider IDE. You can either use the "Run" button for "TasksManagement.Api" at your IDE, or you can use the following commands in the command line:  ` dotnet run `  or  ` dotnet watch run `

- By default, the project will run on [*https://localhost:7282/swagger/index.html*](https://localhost:7282/swagger/index.html)
- When you run the project, it will initially create the "TasksDb" database (if it doesn't exist) and apply the migration for creating the database table

     - NOTE: If you want to view the running SQL Server database locally from SQL Server Management Studio, you can connect to it by using the same details/credentials
        as in the database connection string (check "appsettings.json" file):

- To test the API endpoints, you can use the Swagger UI or Postman, by importing the "Tasks Management API.postman_collection.json" file that I have provided in the solution directory.

- You can use RabbitMQ Management UI to see the queues, exchanges, messages, etc. on [*http://localhost:15672/*](http://localhost:15672/) (use default credentials)