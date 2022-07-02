import express, { Request, Response } from "express";
import { Int32, ObjectId } from "mongodb";
import { collections } from "../services/database.service";
import Player from "../models/player";

export const playersRouter = express.Router();

playersRouter.use(express.json());

playersRouter.get("/", async (req: Request, res: Response) => {
    try {
       const players = (await collections.players?.find({}).toArray()) as unknown as Player[];

        res.status(200).header("Access-Control-Allow-Origin", "*").send(players);
    } catch (error:any) {
        res.status(500).send(error.message);
    }
});

playersRouter.get("/:id", async (req: Request, res: Response) => {
    const id = req?.params?.id;

    try {
        const query = { _id: id };
        const player = (await collections.players.findOne(query)) as unknown as Player;

        if (player) {
            res.status(200).header("Access-Control-Allow-Origin", "*").send(player);
        }
    } catch (error) {
        res.status(404).send(`Unable to find matching document with id: ${req.params.id}`);
    }
});