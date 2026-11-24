# Diagrama de dados do Mom Recipes

## Estado atual

`Recipe` é a raiz do agregado. Cada receita contém passos e referências aos ingredientes que utiliza. Receitas e ingredientes podem partilhar as mesmas etiquetas. O repositório usa PostgreSQL com Entity Framework Core Code First.

```mermaid
classDiagram
    Recipe "1" *-- "0..*" RecipeIngredient : Ingredients
    Recipe "1" *-- "0..*" RecipeStep : Steps
    RecipeIngredient "*" --> "1" Ingredient
    Recipe "1" *-- "0..*" RecipeTag
    Ingredient "1" *-- "0..*" IngredientTag
    RecipeTag "*" --> "1" Tag
    IngredientTag "*" --> "1" Tag

    class Recipe {
        Guid Id
        string Title
        string? Description
        string? ImageUrl
        int? Servings
        int? TotalTimeInMinutes
    }
    class RecipeIngredient {
        decimal? Quantity
        string? Unit
        string? Notes
    }
    class RecipeStep {
        int Position
        string Instruction
    }
```

## Modelo relacional

O esquema é criado pelas migrações do projeto `MomRecipes.Infrastructure`.

```mermaid
erDiagram
    Recipes ||..o{ RecipeIngredients : includes
    Ingredients ||..o{ RecipeIngredients : appears_in
    Recipes ||..o{ RecipeSteps : has
    Recipes ||--o{ RecipeTags : has
    Tags ||--o{ RecipeTags : labels
    Ingredients ||--o{ IngredientTags : has
    Tags ||--o{ IngredientTags : labels

    Recipes {
        Guid Id PK
        string Title
        string Description "opcional"
        string ImageUrl "opcional"
        int Servings "opcional"
        int TotalTimeInMinutes "opcional"
    }

    Ingredients {
        int Id PK
        string Name
        string ImageUrl "opcional"
    }

    RecipeIngredients {
        int Id PK
        Guid RecipeId FK
        int IngredientId FK
        decimal Quantity "opcional"
        string Unit "opcional"
        string Notes "opcional"
    }

    RecipeSteps {
        int Id PK
        Guid RecipeId FK
        int Position
        string Instruction
    }

    Tags {
        int Id PK
        string Name
    }

    RecipeTags {
        Guid RecipeId PK, FK
        int TagId PK, FK
    }

    IngredientTags {
        int IngredientId PK, FK
        int TagId PK, FK
    }
```

- `RecipeIngredients` tem um `Id` próprio para permitir que o mesmo ingrediente apareça mais de uma vez na receita. Quando há uma quantidade mensurável, `Quantity` guarda o valor e `Unit` usa uma unidade consistente, como `g` ou `ml`. Para «sal q.b.», `Notes` guarda «q.b.» e a quantidade e a unidade ficam vazias.
- `RecipeSteps` guarda as instruções pela ordem de preparação. A combinação de `RecipeId` e `Position` deve ser única.
- `RecipeTags` e `IngredientTags` usam chaves compostas para impedir atribuições repetidas. Ambas têm um índice que começa por `TagId` para filtrar por etiqueta.
- As cardinalidades permitem guardar uma receita em rascunho com zero ingredientes, passos ou etiquetas.
