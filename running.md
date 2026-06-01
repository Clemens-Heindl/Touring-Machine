## Running commands so we dont forget
 
Open three terminals and run the following commands:
 
**1 - Docker (PostgreSQL):**
```bash
docker-compose up -d
```
 
**2 - .NET API:**
```bash
cd TourPlannerAPI
dotnet run --launch-profile http
```
 
**3 - Angular Frontend:**
```bash
cd frontend
npm start
```
 
Then open [http://127.0.0.1:4200/](http://127.0.0.1:4200/)
 
## Stopping the App
 
```bash
docker-compose down   # Stops PostgreSQL
# Ctrl+C in the other two terminals
```
 