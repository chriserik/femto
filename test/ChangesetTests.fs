module ChangesetTests

module UserSchema =
    open Femto

    type User = {
        name: string
        age: int
    }

    let changeset model parameters  =
        model
        |> Changeset.cast parameters ["name"]
        |> Changeset.addValidation
            (fun u -> u.age < 20)
            "age"
            "should be greater than 20"
        |> Changeset.addValidation
            (fun u -> u.name <> "")
            "name"
            "should not be empty"
        |> Changeset.validate

open Expecto
open UserSchema

let tests =
    testList "Changeset" [
        testCase "Updates the data on cast" <| fun _ ->
            let user = { name = "demo"; age = 21 }
            let parameters = Map.ofList ["name", "michel" :> obj]
            let ch = UserSchema.changeset user parameters
            Expect.isTrue (ch.data.name = "michel") "changes name"
            Expect.isTrue (user.name = "demo") "does not change name"

        testCase "Validates a changeset" <| fun _ ->
            let user = { name = ""; age = 18 }
            let parameters = Map.ofList ["name", "michel" :> obj]
            let ch = UserSchema.changeset user parameters
            Expect.isFalse ch.valid.Value "invalid changeset"
            Expect.isNonEmpty ch.errors "must have errors"
            Expect.isTrue (Map.count ch.errors = 2) "has 2 errors"
            let fields =
                ch.errors
                |> Map.toList
                |> List.map (fun (k, _) -> k)
            let field1 = List.head fields
            let field2 = List.head (List.tail fields)
            Expect.equal field1 "age" "should be equal"
            Expect.equal field2 "name" "should be equal"
    ]
