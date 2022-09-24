import express, { Request, Response } from "express";
import { Int32, ObjectId } from "mongodb";
import { collections } from "../services/database.service";
import Event from "../models/event";

export const eventsRouter = express.Router();

eventsRouter.use(express.json());

eventsRouter.get("/", async (req: Request, res: Response) => {
    res.status(200).send("");
});

eventsRouter.get("/:id", async (req: Request, res: Response) => {
    const id = req?.params?.id;

    try {
        
        const query = { _id: id };
        const event = (await collections.events.findOne(query)) as unknown as Event;

        if (event) {
            res.status(200).header("Access-Control-Allow-Origin", "*").send(event);
        }
    } catch (error) {
        res.status(404).send(`Unable to find matching document with id: ${req.params.id}`);
    }
});