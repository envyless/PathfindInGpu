using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Flow Maker that make flow with Path Informations
/// when some many characters move to goal that need calcuate each ai, but this use flow buffer.
/// 
/// 1. Make Path Info buffer using brushfire Algorithm from goal
/// 2. and then every character use that buffer. select lowest cost 
/// 
/// so here is following
/// 
/// - path info buffer - contains cost S(from start) + cost G(from goal)
/// - ai buffer - contains index of AI position on path
/// 
/// - ai will call GetDirection() if ai didn't get goal from Result
/// </summary>
public class FlowMaker : MonoBehaviour
{

}
