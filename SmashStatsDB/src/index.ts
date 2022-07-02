import express from "express";
import { connectToDatabase } from "./services/database.service"
import { tournamentsRouter } from "./routes/tournaments.router";
import { eventsRouter } from "./routes/events.router";
import { setsRouter } from "./routes/sets.router";
import { playersRouter } from "./routes/players.router";

const app = express();
const port = 8080; // default port to listen

connectToDatabase()
    .then(() => {
        app.use("/tournaments", tournamentsRouter);
        app.use("/events", eventsRouter);
        app.use("/sets", setsRouter);
        app.use("/players", playersRouter);

        app.listen(port, () => {
            console.log(`Server started at http://localhost:${port}`);
        });
    })
    .catch((error: Error) => {
        console.error("Database connection failed", error);
        process.exit();
    });