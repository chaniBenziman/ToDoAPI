import axios from 'axios';

// Setting the API address as default with the correct port
axios.defaults.baseURL = 'http://localhost:5283';

// Added an interceptor that catches the errors in the response and writes to the log
axios.interceptors.response.use(
  function (response) {
    return response;
  },
  function (error) {
    console.error('Request failed:', error.message);
    return Promise.reject(error);
    
  }
);

const TaskService = {
  getTasks: async () => {
    try {
      const result = await axios.get('/tasks');
      return result.data;
    } catch (error) {
      console.error('Error fetching tasks:', error);
      throw error;
    }
  },

  addTask: async (name) => {
    try {
      const result = await axios.post('/tasks', { name });
      return result.data;
    } catch (error) {
      console.error('Error adding task:', error);
      throw error;
    }
  },

  setCompleted: async (id, isComplete) => {
    try {
      const result = await axios.put(`/task/${id}`, { isComplete });
      return result.data;
    } catch (error) {
      console.error('Error setting task completion:', error);
      throw error;
    }
  },
  
  deleteTask: async (id) => {
  try {
    await axios.delete(`/tasks/${id}`);
    console.log('Task deleted successfully');
  } catch (error) {
    console.error('Error deleting task:', error);
    throw error;
  }
},
};

export default TaskService;