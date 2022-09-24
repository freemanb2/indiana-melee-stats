import * as mongoDB from "mongodb";
import * as dotenv from "dotenv";

export const collections: { 
    tournaments?: mongoDB.Collection,
    events?: mongoDB.Collection,
    sets?: mongoDB.Collection,
    players?: mongoDB.Collection
} = {}

export async function connectToDatabase () {
    dotenv.config();
    console.log(process.env.DB_CONN_STRING);
    const client: mongoDB.MongoClient = new mongoDB.MongoClient(process.env.DB_CONN_STRING);
            
    await client.connect();
        
    const db: mongoDB.Db = client.db(process.env.DB_NAME);
   
    const tournamentsCollection: mongoDB.Collection = db.collection("Tournaments");
    const eventsCollection: mongoDB.Collection = db.collection("Events");
    const setsCollection: mongoDB.Collection = db.collection("Sets");
    const playersCollection: mongoDB.Collection = db.collection("Players");
 
    collections.tournaments = tournamentsCollection;
    collections.events = eventsCollection;
    collections.sets = setsCollection;
    collections.players = playersCollection;
 }