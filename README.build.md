## Building and Running Your Application

### Using Docker

To run the application using Docker, follow these steps:

- **Download Docker:** If Docker isn't installed, get it from [Docker's official website](https://www.docker.com/).


- **Start the Application:** Run the following command in your terminal:
   ```bash
   # First change directories to the location of your Docker Compose file.
   docker compose -f compose-production.yaml up --build
   ```

The application will be available at [http://localhost:80](http://localhost:80).

**Note**: If you want to use a different port than the default `80`, you can achieve that by changing the host port
binding on the reverse proxy container in the compose-production.yaml file:

```yaml
  reverse-proxy:
    image: nginx:1.25.3
    ports:
      - ADD_YOUR_PREFERRED_PORT:80
```

**Note:** The production `compose.yaml` file contains a password setting for the Redis cache. Modify this password based
on your specific needs or preferences.

**Without Docker (Manual Setup)**

- Install `redis-stack-server` or `redis-stack` server
  from [Redis official website](https://redis.io/docs/install/install-stack/).\
  If you want to use Docker for hosting the Redis server (ironically, this is the easiest solution, even for a setup
  without
  Docker), run the following command to spin up a container:
  ```
  docker run -d --name redis-stack -p 6379:6379 -p 8001:8001 redis/redis-stack:6.2.6-v10
  ```

  **Note:** With the above command, you can view the Redis states saved on the server using http://localhost:8001.


- **Install .NET 8:**  
  Ensure you have .NET 8 installed. Download it from the [.NET downloads page](https://dotnet.microsoft.com/download).


- **Download Node.js:**  
  If not already installed, download Node.js from the [Node.js official website](https://nodejs.org/).


- **Start the Server:**  
  After setting up .NET 8, open the solution using your preferred IDE and launch the server. Alternatively, you can run
  the following command:
  ```bash
  dotnet run
  ```
  The server will be available at http://localhost:5114.


- **Start the Client:**  
  Open the client and Run the following command in your terminal:
  ```bash
  npm start
  ```
  The client will be available at http://localhost:3000.
