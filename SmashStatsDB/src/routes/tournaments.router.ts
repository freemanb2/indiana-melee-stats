import express, { Request, Response } from "express";
import { Int32, ObjectId } from "mongodb";
import { collections } from "../services/database.service";
import Tournament from "../models/tournament";

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

tournamentsRouter.get("/:id1/:id2", async (req: Request, res: Response) => {
    const id1 = req?.params?.id1;
    const id2 = req?.params?.id2;
    var tournamentsArray = new Array<any>();
    try {
        const query = { $and: [ { "Events.Sets.Players._id": id1 }, { "Events.Sets.Players._id": id2 } ] };
        (await collections.tournaments.find(query)).forEach((tournament: any) => {
            tournamentsArray.push(tournament);
        }).then(() => {
            if (tournamentsArray) {
                res.status(200).header("Access-Control-Allow-Origin", "*").send(tournamentsArray);
            }
        });
    } catch (error) {
        res.status(404).send(`Unable to find matching document with id: ${req.params.id}`);
    }
});