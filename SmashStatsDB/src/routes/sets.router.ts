import express, { Request, Response } from "express";
import { Int32, ObjectId } from "mongodb";
import { collections } from "../services/database.service";
import Set from "../models/set";

export const setsRouter = express.Router();

setsRouter.use(express.json());

setsRouter.get("/:id", async (req: Request, res: Response) => {
    const id = req?.params?.id;

    try {
        const query = { _id: id };
        const set = (await collections.sets.findOne(query)) as unknown as Set;

        if (set) {
            res.status(200).header("Access-Control-Allow-Origin", "*").send(set);
        }
    } catch (error) {
        res.status(404).send(`Unable to find matching document with id: ${req.params.id}`);
    }
});