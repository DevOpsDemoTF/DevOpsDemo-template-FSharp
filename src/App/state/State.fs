module Service.State

type State =
    {
        config : Config.Configuration
        mutable healthy : bool
    }

let create config =
    {
        config = config
        healthy = true
    } 