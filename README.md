# AnimeWebApi
The project is developed in .NET Core 8 as a Web Api project.

## Anime Hub API

The Anime Hub API enables CRUD operations on the stored animes, as well as implements authentication and authorization functionalities.

### CRUD Operations on Animes

The API includes an anime controller that allows basic data persistence operations on the anime entity. Additionally, it manages the favorite animes stored by users.


### Project Features

#### Namespace Division
The API project is organized for better class management and maintainability. For simplicity and time efficiency, folders within the same project were used instead of separate interconnected projects as is conventionally done.

#### Dependency Injection
The project employs interfaces to define repository operations, decoupling the controllers from the infrastructure classes. These interfaces are configured to integrate with specific classes as needed.

#### Entity Framework
Entity Framework was chosen for data persistence operations to streamline processes. Automatic mapping is handled in the `AnimeDbContext` class.

#### JWT
For security purposes, JSON Web Token (JWT) is used to manage user authentication.

#### Data Encryption
Sensitive data such as user passwords are encrypted to ensure data security in the database.

#### Action Protection
Actions that modify system data are protected by .NET authorization, blocking requests without a valid system token.

#### Role Validation
Since the token is used by any registered user, write operations are restricted to admin users based on the token information. Additionally, the favorites section is exclusive to client users, ensuring admin tokens cannot perform these operations.

#### Testing
An xUnit testing project is included to verify the correct functionality of individual classes, independently evaluating potential system responses.


### Integration of AI

The integration of AI (ChatGPT) was crucial for implementing project features and optimizing the development process. Specifically, encryption functionality, JWT implementation, and testing were primarily developed using AI. It also significantly expedited the code documentation process.

