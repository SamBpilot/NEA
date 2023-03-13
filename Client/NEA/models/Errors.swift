//
//  Errors.swift
//  NEA
//
//  Created by Sam Blewitt on 26/12/2022.
//

import Foundation

enum NEA : Error {
    /// Throw when an invalid IP is entered
    case invalidIP
    
    /// Throw when the Network Connection is invalid
    case cancelled
    
    /// Throw when failed to contact the server
    case failedToContact
    
    /// Throw when a user has entered a station that is not valid
    case invalidStation
    
    /// Throw when an unknown error occurred
    case unkownError
}

extension NEA : CustomStringConvertible {
    public var description: String {
        switch self {
        case .invalidIP:
            return "The IP Address is invalid."
        case .cancelled:
            return "The connection to the IP address was terminated."
        case .failedToContact:
            return "Failed to contact the server, this could be due to your service or the server could be down."
        case .invalidStation:
            return "One or more station you entered was not found on the system."
        case .unkownError:
            return "An unkown error occured. Contact your system administrator for help."
        }
    }
}
