import express, { Request, Response } from "express";
import { Int32, ObjectId, FindCursor, WithId } from "mongodb";
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

setsRouter.get("/:id1/:id2", async (req: Request, res: Response) => {
    const id1 = req?.params?.id1;
    const id2 = req?.params?.id2;
    var setsArray = new Array<any>();

    console.log(id1);
    console.log(id2);

    try {
        const query = { $and: [ { "Players.Player": id1 }, { "Players.Player": id2 } ] };
        (await collections.sets.find(query)).forEach((set: any) => {
            setsArray.push(set);
        }).then(() => {
            if (setsArray) {
                res.status(200).header("Access-Control-Allow-Origin", "*").send(setsArray);
            }
        });
    } catch (error) {
        res.status(404).send(`Unable to find matching document with id: ${req.params.id}`);
    }
});