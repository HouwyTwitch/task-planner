"use strict";(()=>{var g="https://api.linear.app/graphql",c=["completed","canceled"],n=e=>{try{return PluginAPI.translate(e)}catch{return e}},p=`
  query SearchIssues($first: Int!, $team: TeamFilter, $project: NullableProjectFilter) {
    viewer {
      assignedIssues(
        first: $first,
        filter: {
          state: { type: { in: ["backlog", "unstarted", "started"] } },
          team: $team,
          project: $project
        }
      ) {
        nodes {
          id identifier number title updatedAt url
          state { id name type }
        }
      }
    }
  }
`,m=`
  query GetIssue($id: String!) {
    issue(id: $id) {
      id identifier number title description priority
      createdAt updatedAt completedAt canceledAt dueDate url
      state { id name type }
      team { id name key }
      assignee { id name avatarUrl }
      creator { id name }
      labels(first: 50) { nodes { id name color } }
      comments(first: 50) {
        nodes { id body createdAt user { id name avatarUrl } }
      }
    }
  }
`,y="query GetViewer { viewer { id name } }",o=async(e,r,a)=>{let s=await e.post(g,{query:r,variables:a});if(s?.errors?.length)throw new Error(s.errors[0].message||"Linear GraphQL error");if(!s?.data)throw new Error("No data returned from Linear");return s.data},I=e=>({id:e.id,title:`${e.identifier} ${e.title}`,url:e.url,status:e.state?.name,identifier:e.identifier,stateType:e.state?.type}),u=async(e,r,a)=>{let s={first:50};r.teamId&&(s.team={id:{eq:r.teamId}}),r.projectId&&(s.project={id:{eq:r.projectId}});let i=(await o(a,p,s)).viewer?.assignedIssues?.nodes||[],d=e.trim().toLowerCase();return d&&(i=i.filter(l=>l.title.toLowerCase().includes(d)||l.identifier.toLowerCase().includes(d))),i.map(I)};PluginAPI.registerIssueProvider({configFields:[{key:"apiKey",type:"password",label:n("CFG.API_KEY"),required:!0},{key:"apiKeyHelp",type:"link",label:n("CFG.HOW_TO_GET_TOKEN"),url:"https://linear.app/settings/account/security"},{key:"teamId",type:"input",label:n("CFG.TEAM_ID"),advanced:!0},{key:"projectId",type:"input",label:n("CFG.PROJECT_ID"),advanced:!0}],getHeaders(e){return{"Content-Type":"application/json",Authorization:e.apiKey||""}},searchIssues(e,r,a){return u(e,r,a)},async getById(e,r,a){let t=(await o(a,m,{id:e})).issue;if(!t)throw new Error("No issue data returned from Linear");return{id:t.id,title:t.title,body:t.description||"",url:t.url,state:t.state?.name,lastUpdated:new Date(t.updatedAt).getTime(),assignee:t.assignee?.name,labels:(t.labels?.nodes||[]).map(i=>i.name),comments:(t.comments?.nodes||[]).filter(i=>!!i.user).map(i=>({author:i.user.name,body:i.body||"",created:new Date(i.createdAt).getTime(),avatarUrl:i.user.avatarUrl})),identifier:t.identifier,number:t.number,summary:`${t.identifier} ${t.title}`,stateType:t.state?.type,priority:t.priority,creator:t.creator?.name,createdAt:new Date(t.createdAt).getTime(),completedAt:t.completedAt?new Date(t.completedAt).getTime():void 0}},getIssueLink(){return""},async testConnection(e,r){try{return await o(r,y,{}),!0}catch{return!1}},getNewIssuesForBacklog(e,r){return u("",e,r)},issueDisplay:[{field:"summary",label:n("DISPLAY.SUMMARY"),type:"link",linkField:"url"},{field:"state",label:n("DISPLAY.STATE"),type:"text",hideEmpty:!0},{field:"priority",label:n("DISPLAY.PRIORITY"),type:"text",hideEmpty:!0},{field:"assignee",label:n("DISPLAY.ASSIGNEE"),type:"text",hideEmpty:!0},{field:"labels",label:n("DISPLAY.LABELS"),type:"list",hideEmpty:!0},{field:"body",label:n("DISPLAY.DESCRIPTION"),type:"markdown"}],commentsConfig:{authorField:"author",bodyField:"body",createdField:"created",avatarField:"avatarUrl"},fieldMappings:[{taskField:"isDone",issueField:"stateType",defaultDirection:"pullOnly",toIssueValue:e=>e?"completed":"unstarted",toTaskValue:e=>c.includes(e)}],extractSyncValues(e){return{stateType:e.stateType,title:e.title,body:e.body}}});})();
