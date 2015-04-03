namespace qlib

module Async =
    let Map f workflow = async {
        let! res = workflow
        return f res
    }
