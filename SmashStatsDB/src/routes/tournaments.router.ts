import express, { Request, Response } from "express";
import { Int32, ObjectId } from "mongodb";
import { collections } from "../services/database.service";
import Tournament from "../models/tournament";
import Player from "../models/player";
import Event from "../models/event";

export const tournamentsRouter = express.Router();

tournamentsRouter.use(express.json());

tournamentsRouter.get("/recent/:count", async (req: Request, res: Response) => {
    var countString = req?.params?.count;
    const count: number = +countString;

    try {
       const tournaments = (await collections.tournaments?.find({}).limit(count).toArray()) as unknown as Tournament[];

        res.status(200).header("Access-Control-Allow-Origin", "*").send(tournaments);
    } catch (error:any) {
        res.status(500).send(error.message);
    }
});

tournamentsRouter.get("/:id", async (req: Request, res: Response) => {
    const id = req?.params?.id;

    try {
        
        const query = { _id: id };
        const tournament = (await collections.tournaments.findOne(query)) as unknown as Tournament;

        if (tournament) {
            res.status(200).header("Access-Control-Allow-Origin", "*").send(tournament);
        }
    } catch (error) {
        res.status(404).send(`Unable to find matching document with id: ${req.params.id}`);
    }
});

tournamentsRouter.get("/set/:id", async (req: Request, res: Response) => {
    const id = req?.params?.id;

    try {
        
        const query = { "Events.Sets._id": id };
        const tournament = (await collections.tournaments.findOne(query)) as unknown as Tournament;
        console.log(tournament);
        if (tournament) {
            res.status(200).header("Access-Control-Allow-Origin", "*").send(tournament);
        }
    } catch (error) {
        res.status(404).send(`Unable to find matching document with id: ${req.params.id}`);
    }
});